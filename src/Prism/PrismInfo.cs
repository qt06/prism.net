// SPDX-License-Identifier: MPL-2.0

using Prism.Native;

namespace Prism
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    /// <summary>
    /// Runtime information about the native Prism library.
    /// </summary>
    public static class PrismInfo
    {
        /// <summary>
        /// Encoded library version: (major &lt;&lt; 16) | (minor &lt;&lt; 8) | patch.
        /// </summary>
        public static uint Version => PrismNativeLib.prism_version();

        /// <summary>
        /// Human-readable library version string, e.g. "0.18.2".
        /// </summary>
        public static string VersionString => PrismNativeLib.Utf8PtrToString(PrismNativeLib.prism_version_string());

        /// <summary>
        /// The plugin ABI generation implemented by the binding (constant
        /// PRISM_PLUGIN_ABI_VERSION from prism.h).
        /// </summary>
        public const ulong PluginAbiVersion = PrismNativeLib.PRISM_PLUGIN_ABI_VERSION;
    }
}
