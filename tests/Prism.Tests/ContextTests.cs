// SPDX-License-Identifier: MPL-2.0

using System;
using System.Runtime.InteropServices;
using Xunit;
using Prism;
using Prism.Primitives;
using Prism.Exceptions;

namespace Prism.Tests
{
    public class BackendFeaturesTests
    {
        [Fact]
        public void BackendFeatures_HasAllExpectedFlags()
        {
            Assert.Equal(1UL << 0, (ulong)BackendFeatures.IsSupportedAtRuntime);
            Assert.Equal(1UL << 2, (ulong)BackendFeatures.SupportsSpeak);
            Assert.Equal(1UL << 3, (ulong)BackendFeatures.SupportsSpeakToMemory);
            Assert.Equal(1UL << 4, (ulong)BackendFeatures.SupportsBraille);
            Assert.Equal(1UL << 5, (ulong)BackendFeatures.SupportsOutput);
            Assert.Equal(1UL << 6, (ulong)BackendFeatures.SupportsIsSpeaking);
            Assert.Equal(1UL << 7, (ulong)BackendFeatures.SupportsStop);
            Assert.Equal(1UL << 8, (ulong)BackendFeatures.SupportsPause);
            Assert.Equal(1UL << 9, (ulong)BackendFeatures.SupportsResume);
            Assert.Equal(1UL << 10, (ulong)BackendFeatures.SupportsSetVolume);
            Assert.Equal(1UL << 11, (ulong)BackendFeatures.SupportsGetVolume);
            Assert.Equal(1UL << 12, (ulong)BackendFeatures.SupportsSetRate);
            Assert.Equal(1UL << 13, (ulong)BackendFeatures.SupportsGetRate);
            Assert.Equal(1UL << 14, (ulong)BackendFeatures.SupportsSetPitch);
            Assert.Equal(1UL << 15, (ulong)BackendFeatures.SupportsGetPitch);
            Assert.Equal(1UL << 16, (ulong)BackendFeatures.SupportsRefreshVoices);
            Assert.Equal(1UL << 17, (ulong)BackendFeatures.SupportsCountVoices);
            Assert.Equal(1UL << 18, (ulong)BackendFeatures.SupportsGetVoiceName);
            Assert.Equal(1UL << 19, (ulong)BackendFeatures.SupportsGetVoiceLanguage);
            Assert.Equal(1UL << 20, (ulong)BackendFeatures.SupportsGetVoice);
            Assert.Equal(1UL << 21, (ulong)BackendFeatures.SupportsSetVoice);
            Assert.Equal(1UL << 22, (ulong)BackendFeatures.SupportsGetChannels);
            Assert.Equal(1UL << 23, (ulong)BackendFeatures.SupportsGetSampleRate);
            Assert.Equal(1UL << 24, (ulong)BackendFeatures.SupportsGetBitDepth);
            Assert.Equal(1UL << 25, (ulong)BackendFeatures.PerformsSilenceTrimmingOnSpeak);
            Assert.Equal(1UL << 26, (ulong)BackendFeatures.PerformsSilenceTrimmingOnSpeakToMemory);
            Assert.Equal(1UL << 27, (ulong)BackendFeatures.SupportsSpeakSsml);
            Assert.Equal(1UL << 28, (ulong)BackendFeatures.SupportsSpeakToMemorySsml);
            Assert.Equal(1UL << 63, (ulong)BackendFeatures.FeatureMaxBit);
        }
    }

    public class BackendIdTests
    {
        [Fact]
        public void BackendId_HasAllExpectedValues()
        {
            Assert.Equal(0UL, (ulong)BackendId.INVALID);
            Assert.Equal(0x1D6DF72422CEEE66UL, (ulong)BackendId.SAPI);
            Assert.Equal(0x28E3429577805C24UL, (ulong)BackendId.AV_SPEECH);
            Assert.Equal(0xCB4897961A754BCBUL, (ulong)BackendId.VOICE_OVER);
            Assert.Equal(0xE3D6F895D949EBFEUL, (ulong)BackendId.SPEECH_DISPATCHER);
            Assert.Equal(0x89CC19C5C4AC1A56UL, (ulong)BackendId.NVDA);
            Assert.Equal(0xAC3D60E9BD84B53EUL, (ulong)BackendId.JAWS);
            Assert.Equal(0x6797D32F0D994CB4UL, (ulong)BackendId.ONE_CORE);
            Assert.Equal(0x10AA1FC05A17F96CUL, (ulong)BackendId.ORCA);
            Assert.Equal(0xD199C175AEEC494BUL, (ulong)BackendId.ANDROID_SCREEN_READER);
            Assert.Equal(0xBC175831BFE4E5CCUL, (ulong)BackendId.ANDROID_TTS);
            Assert.Equal(0x3572538D44D44A8FUL, (ulong)BackendId.WEB_SPEECH);
            Assert.Equal(0x6238F019DB678F8EUL, (ulong)BackendId.UIA);
            Assert.Equal(0x3D93C56C9E7F2A2EUL, (ulong)BackendId.ZDSR);
            Assert.Equal(0xAE439D62DC7B1479UL, (ulong)BackendId.ZOOM_TEXT);
            Assert.Equal(0x285aba1c16f3300fUL, (ulong)BackendId.BOY_PC_READER);
            Assert.Equal(0x344B951962E3B835UL, (ulong)BackendId.PC_TALKER);
            Assert.Equal(0x8380F2A37B2C3EB6UL, (ulong)BackendId.SYSTEM_ACCESS);
            Assert.Equal(0x9120D89908785C13UL, (ulong)BackendId.WINDOW_EYES);
            Assert.Equal(0xED4760890B55C2F2UL, (ulong)BackendId.SENSE_READER);
            Assert.Equal(0x478B44F14AD3D89CUL, (ulong)BackendId.SPIEL);
            Assert.Equal(21, Enum.GetValues(typeof(BackendId)).Length);
        }
    }

    public class PrismErrorTests
    {
        [Fact]
        public void PrismError_HasAllExpectedValues()
        {
            Assert.Equal(0U, (uint)PrismError.OK);
            Assert.Equal(1U, (uint)PrismError.NotInitialized);
            Assert.Equal(2U, (uint)PrismError.InvalidParam);
            Assert.Equal(3U, (uint)PrismError.NotImplemented);
            Assert.Equal(4U, (uint)PrismError.NoVoices);
            Assert.Equal(5U, (uint)PrismError.VoiceNotFound);
            Assert.Equal(6U, (uint)PrismError.SpeakFailure);
            Assert.Equal(7U, (uint)PrismError.MemoryFailure);
            Assert.Equal(8U, (uint)PrismError.RangeOutOfBounds);
            Assert.Equal(9U, (uint)PrismError.Internal);
            Assert.Equal(10U, (uint)PrismError.NotSpeaking);
            Assert.Equal(11U, (uint)PrismError.NotPaused);
            Assert.Equal(12U, (uint)PrismError.AlreadyPaused);
            Assert.Equal(13U, (uint)PrismError.InvalidUtf8);
            Assert.Equal(14U, (uint)PrismError.InvalidOperation);
            Assert.Equal(15U, (uint)PrismError.AlreadyInitialized);
            Assert.Equal(16U, (uint)PrismError.BackendNotAvailable);
            Assert.Equal(17U, (uint)PrismError.Unknown);
            Assert.Equal(18U, (uint)PrismError.InvalidAudioFormat);
            Assert.Equal(19U, (uint)PrismError.InternalBackendLimitExceeded);
            Assert.Equal(20U, (uint)PrismError.BackendEnteredUndefinedState);
            Assert.Equal(21U, (uint)PrismError.LibraryLoadFailed);
            Assert.Equal(22U, (uint)PrismError.LibraryInvalid);
            Assert.Equal(23U, (uint)PrismError.IncompatibleAbi);
            Assert.Equal(24U, (uint)PrismError.Count);
            Assert.Equal(25, Enum.GetValues(typeof(PrismError)).Length);
        }
    }

    public class PrismConfigTests
    {
        [Fact]
        public void PrismConfig_ConfigVersion_MatchesNativeHeader()
        {
            Assert.Equal(4, PrismConfig.ConfigVersion);
        }

        [Fact]
        public void PrismConfig_ToNative_DefaultsVersionToConfigVersion()
        {
            var config = new PrismConfig();
            var native = config.ToNative();
            Assert.Equal(PrismConfig.ConfigVersion, native.version);
        }

        [Fact]
        public void PrismConfig_ToNative_BaselineCallbackDefaultsToNull()
        {
            var config = new PrismConfig();
            var native = config.ToNative();
            Assert.Equal(IntPtr.Zero, native.availabilityBaselineCallback);
        }

        [Fact]
        public void PrismConfigInternal_Layout_MatchesNativeHeader()
        {
            // PrismConfig gained availability_baseline_callback in version 4;
            // prism_config_init() returns the struct by value, so the managed
            // layout must match the native sizeof(PrismConfig) exactly.
            var expected = IntPtr.Size == 8 ? 56 : 36;
            Assert.Equal(expected, Marshal.SizeOf(typeof(Prism.Native.PrismNativeLib.PrismConfigInternal)));
        }
    }

    public class ExceptionMappingTests
    {
        [Fact]
        public void AllErrorCodes_MapToCorrectExceptionTypes()
        {
            Assert.IsType<PrismNotInitializedException>(Backend.CreateException((uint)PrismError.NotInitialized));
            Assert.IsType<PrismInvalidParamException>(Backend.CreateException((uint)PrismError.InvalidParam));
            Assert.IsType<PrismNotImplementedException>(Backend.CreateException((uint)PrismError.NotImplemented));
            Assert.IsType<PrismNoVoicesException>(Backend.CreateException((uint)PrismError.NoVoices));
            Assert.IsType<PrismVoiceNotFoundException>(Backend.CreateException((uint)PrismError.VoiceNotFound));
            Assert.IsType<PrismSpeakException>(Backend.CreateException((uint)PrismError.SpeakFailure));
            Assert.IsType<PrismMemoryException>(Backend.CreateException((uint)PrismError.MemoryFailure));
            Assert.IsType<PrismRangeException>(Backend.CreateException((uint)PrismError.RangeOutOfBounds));
            Assert.IsType<PrismInternalException>(Backend.CreateException((uint)PrismError.Internal));
            Assert.IsType<PrismNotSpeakingException>(Backend.CreateException((uint)PrismError.NotSpeaking));
            Assert.IsType<PrismNotPausedException>(Backend.CreateException((uint)PrismError.NotPaused));
            Assert.IsType<PrismAlreadyPausedException>(Backend.CreateException((uint)PrismError.AlreadyPaused));
            Assert.IsType<PrismInvalidUtf8Exception>(Backend.CreateException((uint)PrismError.InvalidUtf8));
            Assert.IsType<PrismInvalidOperationException>(Backend.CreateException((uint)PrismError.InvalidOperation));
            Assert.IsType<PrismAlreadyInitializedException>(Backend.CreateException((uint)PrismError.AlreadyInitialized));
            Assert.IsType<PrismBackendNotAvailableException>(Backend.CreateException((uint)PrismError.BackendNotAvailable));
            Assert.IsType<PrismUnknownException>(Backend.CreateException((uint)PrismError.Unknown));
            Assert.IsType<PrismInvalidAudioFormatException>(Backend.CreateException((uint)PrismError.InvalidAudioFormat));
            Assert.IsType<PrismInternalBackendLimitExceededException>(Backend.CreateException((uint)PrismError.InternalBackendLimitExceeded));
            Assert.IsType<PrismBackendEnteredUndefinedStateException>(Backend.CreateException((uint)PrismError.BackendEnteredUndefinedState));
            Assert.IsType<PrismLibraryLoadFailedException>(Backend.CreateException((uint)PrismError.LibraryLoadFailed));
            Assert.IsType<PrismLibraryInvalidException>(Backend.CreateException((uint)PrismError.LibraryInvalid));
            Assert.IsType<PrismIncompatibleAbiException>(Backend.CreateException((uint)PrismError.IncompatibleAbi));
        }

        [Fact]
        public void UnknownErrorCode_ReturnsPrismUnknownException()
        {
            var ex = Backend.CreateException(99);
            Assert.IsType<PrismUnknownException>(ex);
        }
    }
}
