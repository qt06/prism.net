// SPDX-License-Identifier: MPL-2.0

using System;
using Prism.Native;

namespace Prism.Primitives
{
    public sealed class PrismConfig
    {
        public byte Version { get; set; }
        public PrismRegistry? Registry { get; set; }
        public Action<BackendId, string, bool>? AvailabilityCallback { get; set; }
        public Action? AvailabilityBaselineCallback { get; set; }
        public uint AvailabilityPollIntervalMs { get; set; }
        public uint AvailabilityDebounceSamples { get; set; }
        public uint AvailabilityBackoffMaxMs { get; set; }
        public bool AvailabilityAutoPowerManage { get; set; }

        public const byte ConfigVersion = PrismNativeLib.PRISM_CONFIG_VERSION;

        internal PrismNativeLib.PrismConfigInternal ToNative()
        {
            return new PrismNativeLib.PrismConfigInternal
            {
                version = Version == 0 ? ConfigVersion : Version,
                registry = Registry?.Handle ?? IntPtr.Zero,
                availabilityCallback = IntPtr.Zero,
                availabilityUserdata = IntPtr.Zero,
                availabilityBaselineCallback = IntPtr.Zero,
                availabilityPollIntervalMs = AvailabilityPollIntervalMs,
                availabilityDebounceSamples = AvailabilityDebounceSamples,
                availabilityBackoffMaxMs = AvailabilityBackoffMaxMs,
                availabilityAutoPowerManage = (byte)(AvailabilityAutoPowerManage ? 1 : 0)
            };
        }

        public static PrismConfig Create()
        {
            var native = PrismNativeLib.prism_config_init();
            return new PrismConfig
            {
                Version = native.version,
                AvailabilityPollIntervalMs = native.availabilityPollIntervalMs,
                AvailabilityDebounceSamples = native.availabilityDebounceSamples,
                AvailabilityBackoffMaxMs = native.availabilityBackoffMaxMs,
                AvailabilityAutoPowerManage = native.availabilityAutoPowerManage != 0
            };
        }
    }
}
