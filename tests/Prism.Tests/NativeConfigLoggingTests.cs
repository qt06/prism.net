// SPDX-License-Identifier: MPL-2.0

using System;
using System.Threading;
using Prism.Primitives;
using Xunit;

namespace Prism.Tests
{
    public class NativeConfigLoggingTests
    {
        [NativeFact]
        public void PrismInfo_ReportsVersion()
        {
            Assert.False(string.IsNullOrEmpty(PrismInfo.VersionString));
            Assert.NotEqual(0u, PrismInfo.Version);
        }

        [NativeFact]
        public void PrismConfig_Create_ReturnsInitializedConfig()
        {
            var config = PrismConfig.Create();
            Assert.Equal(PrismConfig.ConfigVersion, config.Version);
        }

        [NativeFact]
        public void IsAutoPowerSupported_ReturnsBool()
        {
            _ = Context.IsAutoPowerSupported();
        }

        [NativeFact]
        public void PollPauseResume_DoesNotThrow()
        {
            using var context = new Context();
            context.AvailabilityPollPause();
            context.AvailabilityPollResume();
        }

        [NativeFact]
        public void AvailabilityBaseline_IsInvoked()
        {
            using var baselineSeen = new ManualResetEventSlim(false);
            var config = PrismConfig.Create();
            config.AvailabilityCallback = (_, _, _) => { };
            config.AvailabilityBaselineCallback = () => baselineSeen.Set();
            config.AvailabilityPollIntervalMs = 50;

            using var context = new Context(config);
            Assert.True(baselineSeen.Wait(TimeSpan.FromSeconds(5)), "baseline callback was not invoked");
        }

        [NativeFact]
        public void Logger_SetLevelFlush_DoesNotThrow()
        {
            var previous = Logger.SetLevel(PrismLogLevel.Warn);
            try
            {
                Logger.Flush();
            }
            finally
            {
                Logger.SetLevel(previous);
            }
        }
    }
}
