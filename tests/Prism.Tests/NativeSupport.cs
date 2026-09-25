// SPDX-License-Identifier: MPL-2.0

using System;
using Xunit;

namespace Prism.Tests
{
    /// <summary>
    /// Probes whether the native prism library can be loaded. Integration
    /// tests are skipped (rather than failed) when it is unavailable, so the
    /// suite still runs on machines without a native build.
    /// </summary>
    internal static class NativeProbe
    {
        private static readonly object Gate = new object();
        private static bool _probed;
        private static bool _available;
        private static string _reason = "native prism library not available";

        public static bool IsAvailable
        {
            get
            {
                lock (Gate)
                {
                    if (!_probed)
                    {
                        _probed = true;
                        try
                        {
                            // Probe through the same DllImport path the tests use,
                            // so this works on both .NET and .NET Framework.
                            _ = PrismInfo.Version;
                            _available = true;
                        }
                        catch (DllNotFoundException)
                        {
                            _reason = "native prism library (prism.dll) not found";
                        }
                        catch (EntryPointNotFoundException)
                        {
                            _reason = "native prism library does not expose the expected entry points";
                        }
                        catch (BadImageFormatException)
                        {
                            _reason = "native prism library has an incompatible architecture";
                        }
                    }
                    return _available;
                }
            }
        }

        public static string SkipReason => _reason;
    }

    /// <summary>
    /// A <see cref="FactAttribute"/> that runs only when the native prism
    /// library is available, mirroring the behaviour the demos used to provide.
    /// </summary>
    public sealed class NativeFactAttribute : FactAttribute
    {
        public NativeFactAttribute()
        {
            if (!NativeProbe.IsAvailable)
            {
                Skip = NativeProbe.SkipReason;
            }
        }
    }
}
