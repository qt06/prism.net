// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prism.Native;
using Prism.Primitives;

namespace Prism
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    public static class Logger
    {
        private static readonly object Gate = new object();
        private static PrismNativeLib.LogCallbackDelegate? _pinnedNativeCallback;
        private static Action<PrismLogLevel, string, string>? _userCallback;
        private static readonly List<PrismNativeLib.LogCallbackDelegate> RetiredCallbacks =
            new List<PrismNativeLib.LogCallbackDelegate>();

        public static PrismLogLevel SetLevel(PrismLogLevel level)
        {
            return (PrismLogLevel)PrismNativeLib.prism_set_log_level((int)level);
        }

        public static PrismLogLevel Level
        {
            set => PrismNativeLib.prism_set_log_level((int)value);
        }

        public static Action<PrismLogLevel, string, string>? SetHandler(Action<PrismLogLevel, string, string>? handler)
        {
            lock (Gate)
            {
                var previous = _userCallback;
                if (_pinnedNativeCallback != null)
                {
                    // Native logging may still be delivering an in-flight message
                    // through the previous handler after this method returns.
                    RetiredCallbacks.Add(_pinnedNativeCallback);
                }
                if (handler == null)
                {
                    PrismNativeLib.prism_set_log_handler(new PrismNativeLib.PrismLogHandlerInternal
                    {
                        fn = IntPtr.Zero,
                        userdata = IntPtr.Zero
                    });
                    _pinnedNativeCallback = null;
                    _userCallback = null;
                    return previous;
                }

                PrismNativeLib.LogCallbackDelegate nativeCallback = (IntPtr userdata, int level, IntPtr source, IntPtr message) =>
                {
                    try
                    {
                        handler((PrismLogLevel)level,
                                PrismNativeLib.Utf8PtrToString(source),
                                PrismNativeLib.Utf8PtrToString(message));
                    }
                    catch
                    {
                        // The log callback MUST NOT throw across the native boundary.
                    }
                };

                PrismNativeLib.prism_set_log_handler(new PrismNativeLib.PrismLogHandlerInternal
                {
                    fn = Marshal.GetFunctionPointerForDelegate(nativeCallback),
                    userdata = IntPtr.Zero
                });
                // Root both the native thunk and the user callback so the GC
                // cannot collect them while the native library holds the handler.
                _pinnedNativeCallback = nativeCallback;
                _userCallback = handler;
                return previous;
            }
        }

        public static void Log(PrismLogLevel level, string source, string message)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var sourcePtr = PrismNativeLib.StringToUtf8Ptr(source);
            var messagePtr = PrismNativeLib.StringToUtf8Ptr(message);
            try
            {
                PrismNativeLib.prism_log((int)level, sourcePtr, messagePtr);
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(sourcePtr);
                PrismNativeLib.FreeUtf8Ptr(messagePtr);
            }
        }

        public static void Flush()
        {
            PrismNativeLib.prism_log_flush();
        }

        public static void Shutdown()
        {
            PrismNativeLib.prism_log_shutdown();
        }
    }
}
