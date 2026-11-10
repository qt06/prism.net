// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prism.Exceptions;
using Prism.Native;

namespace Prism.Primitives
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    /// <summary>
    /// Builder for a custom Prism registry. A builder is live until it is
    /// successfully frozen; after that it is spent and must be disposed.
    /// </summary>
    public sealed class RegistryBuilder : IDisposable
    {
        private IntPtr _builder;
        private bool _disposed;
        private bool _frozen;
        private readonly List<Delegate> _rootedDelegates = new List<Delegate>();

        public RegistryBuilder()
        {
            _builder = PrismNativeLib.prism_registry_builder_new();
            if (_builder == IntPtr.Zero)
            {
                throw new PrismMemoryException("Failed to create Prism registry builder");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                PrismNativeLib.prism_registry_builder_free(_builder);
                _builder = IntPtr.Zero;
                _rootedDelegates.Clear();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new PrismInvalidOperationException("RegistryBuilder has been disposed");
            }
        }

        private void ThrowIfSpent()
        {
            if (_frozen)
            {
                throw new PrismInvalidOperationException("RegistryBuilder has already been frozen");
            }
        }

        /// <summary>
        /// Registers a custom backend. The vtable delegates and
        /// <paramref name="userdata"/> must stay valid for as long as the
        /// registration is referenced (the frozen registry or any backend
        /// instance created from it); the builder roots the delegates
        /// internally and transfers that responsibility to the frozen
        /// <see cref="PrismRegistry"/> on <see cref="Freeze"/>.
        /// </summary>
        public BackendId AddBackend(string name, int priority, BackendFeatures features, BackendVTable vtable, IntPtr userdata = default)
        {
            ThrowIfDisposed();
            ThrowIfSpent();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Backend name must be a non-empty string", nameof(name));
            }
            if (vtable == null)
            {
                throw new ArgumentNullException(nameof(vtable));
            }
            if (priority < 0)
            {
                throw new PrismRangeException("Priority must be non-negative");
            }

            var namePtr = PrismNativeLib.StringToUtf8Ptr(name);
            var thunks = new List<Delegate>();
            var native = vtable.ToNative(thunks);
            var featureBits = (ulong)features;
            if (!IsConsistent(featureBits, native))
            {
                throw new ArgumentException(
                    "Feature declaration mismatch: every declared feature must have a non-null vtable member, " +
                    "and every non-null vtable member must have its feature declared.", nameof(features));
            }
            try
            {
                var result = PrismNativeLib.prism_registry_builder_add_backend(
                    _builder, namePtr, priority, (ulong)features, ref native, userdata, IntPtr.Zero, out var outId);
                ThrowIfError(result);
                // Root the thunks only after the registration succeeds; a
                // failed registration must not retain the closures.
                _rootedDelegates.AddRange(thunks);
                return (BackendId)outId;
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(namePtr);
            }
        }

        /// <summary>
        /// Loads backends from a Prism plugin shared library. Returns the
        /// number of backends added.
        /// </summary>
        public int AddLibrary(string path, int priorityOverride = -1)
        {
            ThrowIfDisposed();
            ThrowIfSpent();
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Plugin path must be a non-empty string", nameof(path));
            }
            var pathPtr = PrismNativeLib.StringToUtf8Ptr(path);
            try
            {
                var result = PrismNativeLib.prism_registry_builder_add_library(_builder, pathPtr, priorityOverride, out var outCount);
                ThrowIfError(result);
                return (int)outCount;
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(pathPtr);
            }
        }

        /// <summary>
        /// Freezes the builder into an immutable registry. The builder becomes
        /// spent and must still be disposed. Ownership of the rooted custom
        /// backend delegates transfers to the returned registry, which must be
        /// kept alive while backends created from it are in use.
        /// </summary>
        public PrismRegistry Freeze()
        {
            ThrowIfDisposed();
            ThrowIfSpent();
            var registry = PrismNativeLib.prism_registry_freeze(_builder);
            if (registry == IntPtr.Zero)
            {
                throw new PrismMemoryException("Failed to freeze Prism registry");
            }
            _frozen = true;
            var rooted = new List<Delegate>(_rootedDelegates);
            _rootedDelegates.Clear();
            return new PrismRegistry(registry, rooted);
        }

        private static void ThrowIfError(uint error)
        {
            if (error == PrismNativeLib.PRISM_OK)
            {
                return;
            }
            throw Prism.Backend.CreateException(error);
        }

        private static bool IsConsistent(ulong features, PrismNativeLib.PrismBackendVTableInternal vtable)
        {
            return Has(features, BackendFeatures.SupportsSpeak, vtable.speak)
                && Has(features, BackendFeatures.SupportsSpeakToMemory, vtable.speak_to_memory)
                && Has(features, BackendFeatures.SupportsBraille, vtable.braille)
                && Has(features, BackendFeatures.SupportsOutput, vtable.output)
                && Has(features, BackendFeatures.SupportsIsSpeaking, vtable.is_speaking)
                && Has(features, BackendFeatures.SupportsStop, vtable.stop)
                && Has(features, BackendFeatures.SupportsPause, vtable.pause)
                && Has(features, BackendFeatures.SupportsResume, vtable.resume)
                && Has(features, BackendFeatures.SupportsSetVolume, vtable.set_volume)
                && Has(features, BackendFeatures.SupportsGetVolume, vtable.get_volume)
                && Has(features, BackendFeatures.SupportsSetRate, vtable.set_rate)
                && Has(features, BackendFeatures.SupportsGetRate, vtable.get_rate)
                && Has(features, BackendFeatures.SupportsSetPitch, vtable.set_pitch)
                && Has(features, BackendFeatures.SupportsGetPitch, vtable.get_pitch)
                && Has(features, BackendFeatures.SupportsRefreshVoices, vtable.refresh_voices)
                && Has(features, BackendFeatures.SupportsCountVoices, vtable.count_voices)
                && Has(features, BackendFeatures.SupportsGetVoiceName, vtable.get_voice_name)
                && Has(features, BackendFeatures.SupportsGetVoiceLanguage, vtable.get_voice_language)
                && Has(features, BackendFeatures.SupportsGetVoice, vtable.get_voice)
                && Has(features, BackendFeatures.SupportsSetVoice, vtable.set_voice)
                && Has(features, BackendFeatures.SupportsGetChannels, vtable.get_channels)
                && Has(features, BackendFeatures.SupportsGetSampleRate, vtable.get_sample_rate)
                && Has(features, BackendFeatures.SupportsGetBitDepth, vtable.get_bit_depth);
        }

        private static bool Has(ulong features, BackendFeatures feature, IntPtr member)
        {
            var present = member != IntPtr.Zero;
            var declared = (features & (ulong)feature) != 0;
            return present == declared;
        }
    }
}
