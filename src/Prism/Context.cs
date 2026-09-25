// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prism.Native;
using Prism.Primitives;
using Prism.Exceptions;

namespace Prism
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    public sealed class Context : IDisposable
    {
        private IntPtr _context;
        private bool _disposed;
        private readonly object _backendsGate = new object();
        private readonly List<Backend> _backends = new List<Backend>();
        private readonly List<Delegate> _registryDelegates = new List<Delegate>();
        private readonly PrismNativeLib.AvailabilityCallbackDelegate? _availabilityCallback;
        private readonly PrismNativeLib.AvailabilityBaselineCallbackDelegate? _availabilityBaselineCallback;

        public Context() : this(PrismConfig.Create())
        {
        }

        public Context(PrismConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            var nativeConfig = config.ToNative();
            if (config.Registry != null)
            {
                _registryDelegates.AddRange(config.Registry.RootedDelegates);
            }
            if (config.AvailabilityCallback != null)
            {
                var userCallback = config.AvailabilityCallback;
                PrismNativeLib.AvailabilityCallbackDelegate nativeCallback = (IntPtr userdata, ulong backend, IntPtr name, bool available) =>
                {
                    try
                    {
                        userCallback((BackendId)backend, PrismNativeLib.Utf8PtrToString(name), available);
                    }
                    catch
                    {
                        // The availability callback runs on a background
                        // polling thread and must not throw across the
                        // native boundary.
                    }
                };
                // Rooted for the lifetime of this Context; the native
                // enumerator invokes it from a background thread.
                _availabilityCallback = nativeCallback;
                nativeConfig.availabilityCallback =
                    Marshal.GetFunctionPointerForDelegate(nativeCallback);

                if (config.AvailabilityBaselineCallback != null)
                {
                    var userBaselineCallback = config.AvailabilityBaselineCallback;
                    PrismNativeLib.AvailabilityBaselineCallbackDelegate nativeBaselineCallback = (IntPtr userdata) =>
                    {
                        try
                        {
                            userBaselineCallback();
                        }
                        catch
                        {
                            // The baseline callback shares the poll thread with
                            // the availability callback and must not throw
                            // across the native boundary.
                        }
                    };
                    // Rooted for the lifetime of this Context; the native
                    // enumerator invokes it once from the poll thread.
                    _availabilityBaselineCallback = nativeBaselineCallback;
                    nativeConfig.availabilityBaselineCallback =
                        Marshal.GetFunctionPointerForDelegate(nativeBaselineCallback);
                }
            }
            _context = PrismNativeLib.prism_init(ref nativeConfig);
            if (_context == IntPtr.Zero)
            {
                throw new PrismInitializationException(
                    "Failed to initialize Prism context. This can happen when the config version is invalid, " +
                    "when COM initialization fails on Windows, or when memory allocation fails.");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                List<Backend> snapshot;
                lock (_backendsGate)
                {
                    snapshot = new List<Backend>(_backends);
                }
                for (var i = snapshot.Count - 1; i >= 0; i--)
                {
                    var backend = snapshot[i];
                    if (backend != null)
                    {
                        backend.Dispose();
                    }
                }
                PrismNativeLib.prism_shutdown(_context);
                _context = IntPtr.Zero;
                _registryDelegates.Clear();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new PrismInvalidOperationException("Context has been disposed");
            }
        }

        internal void RegisterBackend(Backend backend)
        {
            lock (_backendsGate)
            {
                _backends.Add(backend);
            }
        }

        internal void UnregisterBackend(Backend backend)
        {
            lock (_backendsGate)
            {
                _backends.Remove(backend);
            }
        }

        public int BackendsCount
        {
            get
            {
                ThrowIfDisposed();
                return (int)PrismNativeLib.prism_registry_count(_context);
            }
        }

        public BackendId IdAt(int index)
        {
            ThrowIfDisposed();
            if (index < 0 || index >= BackendsCount)
            {
                throw new PrismRangeException($"Index {index} out of range");
            }
            return (BackendId)PrismNativeLib.prism_registry_id_at(_context, (UIntPtr)index);
        }

        public BackendId IdOf(string name)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Backend name must be a non-empty string", nameof(name));
            }
            var namePtr = PrismNativeLib.StringToUtf8Ptr(name);
            try
            {
                var id = PrismNativeLib.prism_registry_id(_context, namePtr);
                if (id == (ulong)BackendId.INVALID)
                {
                    throw new PrismBackendNotAvailableException($"Backend '{name}' not found");
                }
                return (BackendId)id;
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(namePtr);
            }
        }

        public string NameOf(BackendId id)
        {
            ThrowIfDisposed();
            var ptr = PrismNativeLib.prism_registry_name(_context, (ulong)id);
            if (ptr == IntPtr.Zero)
            {
                throw new PrismBackendNotAvailableException($"Backend ID {(ulong)id} not found");
            }
            return PrismNativeLib.Utf8PtrToString(ptr);
        }

        public int PriorityOf(BackendId id)
        {
            ThrowIfDisposed();
            var priority = PrismNativeLib.prism_registry_priority(_context, (ulong)id);
            if (priority < 0)
            {
                throw new PrismBackendNotAvailableException($"Backend ID {(ulong)id} not found");
            }
            return priority;
        }

        public bool Exists(BackendId id)
        {
            ThrowIfDisposed();
            return PrismNativeLib.prism_registry_exists(_context, (ulong)id);
        }

        public Backend Create(BackendId id)
        {
            ThrowIfDisposed();
            var ptr = PrismNativeLib.prism_registry_create(_context, (ulong)id);
            if (ptr == IntPtr.Zero)
            {
                throw new PrismBackendNotAvailableException($"Failed to create backend {(ulong)id}");
            }
            return new Backend(this, ptr);
        }

        public Backend CreateBest()
        {
            ThrowIfDisposed();
            var ptr = PrismNativeLib.prism_registry_create_best(_context);
            if (ptr == IntPtr.Zero)
            {
                throw new PrismBackendNotAvailableException("No available backend could be created");
            }
            return new Backend(this, ptr);
        }

        public Backend Acquire(BackendId id)
        {
            ThrowIfDisposed();
            var ptr = PrismNativeLib.prism_registry_acquire(_context, (ulong)id);
            if (ptr == IntPtr.Zero)
            {
                throw new PrismBackendNotAvailableException($"Failed to acquire backend {(ulong)id}");
            }
            return new Backend(this, ptr);
        }

        public Backend AcquireBest()
        {
            ThrowIfDisposed();
            var ptr = PrismNativeLib.prism_registry_acquire_best(_context);
            if (ptr == IntPtr.Zero)
            {
                throw new PrismBackendNotAvailableException("No available backend could be acquired");
            }
            return new Backend(this, ptr);
        }

        public Backend Get(BackendId id)
        {
            ThrowIfDisposed();
            var ptr = PrismNativeLib.prism_registry_get(_context, (ulong)id);
            if (ptr == IntPtr.Zero)
            {
                throw new PrismBackendNotAvailableException($"No cached backend instance for ID {(ulong)id}");
            }
            return new Backend(this, ptr);
        }

        public void AvailabilityPollPause()
        {
            ThrowIfDisposed();
            PrismNativeLib.prism_availability_poll_pause(_context);
        }

        public void AvailabilityPollResume()
        {
            ThrowIfDisposed();
            PrismNativeLib.prism_availability_poll_resume(_context);
        }

        public static bool IsAutoPowerSupported()
        {
            return PrismNativeLib.prism_availability_auto_power_supported();
        }
    }
}
