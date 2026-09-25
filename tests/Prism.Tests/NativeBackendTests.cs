// SPDX-License-Identifier: MPL-2.0

using System;
using Prism.Exceptions;
using Prism.Primitives;
using Xunit;

namespace Prism.Tests
{
    public class NativeBackendTests
    {
        private static Backend AcquirePreferred(Context context)
            => context.Exists(BackendId.SAPI) ? context.Acquire(BackendId.SAPI) : context.AcquireBest();

        private static void TolerateNotImplemented(Action action)
        {
            try
            {
                action();
            }
            catch (PrismNotImplementedException)
            {
            }
        }

        [NativeFact]
        public void Speak_And_Stop_CompletesOrNotImplemented()
        {
            using var context = new Context();
            using var backend = AcquirePreferred(context);
            TolerateNotImplemented(() =>
            {
                backend.Speak("Prism .NET integration test");
                backend.Stop();
            });
        }

        [NativeFact]
        public void IsSpeaking_ReturnsBool()
        {
            using var context = new Context();
            using var backend = AcquirePreferred(context);
            TolerateNotImplemented(() => _ = backend.IsSpeaking);
        }

        [NativeFact]
        public void Parameters_SetAndRoundTrip()
        {
            using var context = new Context();
            using var backend = AcquirePreferred(context);
            TolerateNotImplemented(() =>
            {
                backend.Volume = 0.5f;
                backend.Rate = 0.5f;
                backend.Pitch = 0.5f;

                var features = backend.Features;
                if (features.HasFlag(BackendFeatures.SupportsGetVolume))
                {
                    Assert.InRange(backend.Volume, 0f, 1f);
                }
                if (features.HasFlag(BackendFeatures.SupportsGetRate))
                {
                    Assert.InRange(backend.Rate, 0f, 1f);
                }
                if (features.HasFlag(BackendFeatures.SupportsGetPitch))
                {
                    Assert.InRange(backend.Pitch, 0f, 1f);
                }
            });
        }

        [NativeFact]
        public void RefreshVoices_Enumerates()
        {
            using var context = new Context();
            using var backend = AcquirePreferred(context);
            try
            {
                backend.RefreshVoices();
                var count = backend.VoicesCount;
                Assert.True(count >= 0);
                if (count > 0)
                {
                    Assert.False(string.IsNullOrEmpty(backend.GetVoiceName(0)));
                }
            }
            catch (PrismNotImplementedException)
            {
            }
            catch (PrismNoVoicesException)
            {
            }
        }

        [NativeFact]
        public void AudioInfo_Reads()
        {
            using var context = new Context();
            using var backend = AcquirePreferred(context);
            TolerateNotImplemented(() =>
            {
                Assert.True(backend.Channels >= 0);
                Assert.True(backend.SampleRate >= 0);
                Assert.True(backend.BitDepth >= 0);
            });
        }

        [NativeFact]
        public void SpeakToMemory_CompletesOrNotImplemented()
        {
            using var context = new Context();
            using var backend = AcquirePreferred(context);
            TolerateNotImplemented(() =>
            {
                backend.SpeakToMemory("Prism .NET integration test",
                    (samples, channels, rate) => { });
            });
        }
    }
}
