// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using Prism.Exceptions;
using Prism.Native;

namespace Prism.Primitives
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    /// <summary>
    /// Wraps an immutable Prism registry. Holds one reference to the native
    /// registry; releasing the wrapper does not affect contexts that have
    /// retained it, nor backend instances created from it.
    /// </summary>
    public sealed class PrismRegistry : IDisposable
    {
        private IntPtr _registry;
        private bool _disposed;

        internal PrismRegistry(IntPtr retainedHandle, List<Delegate> rootedDelegates)
        {
            _registry = retainedHandle;
            // Custom-backend vtable thunks must stay alive as long as the
            // registration is referenced by this registry (or backends created
            // from it), so they are rooted here.
            RootedDelegates = rootedDelegates ?? new List<Delegate>();
        }

        internal IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();
                return _registry;
            }
        }

        internal List<Delegate> RootedDelegates { get; }

        public void Dispose()
        {
            if (!_disposed)
            {
                PrismNativeLib.prism_registry_release(_registry);
                _registry = IntPtr.Zero;
                _disposed = true;
                RootedDelegates.Clear();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new PrismInvalidOperationException("PrismRegistry has been disposed");
            }
        }
    }
}
