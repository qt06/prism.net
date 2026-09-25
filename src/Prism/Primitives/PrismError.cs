// SPDX-License-Identifier: MPL-2.0

namespace Prism.Primitives
{
    public enum PrismError : uint
    {
        OK = 0,
        NotInitialized = 1,
        InvalidParam = 2,
        NotImplemented = 3,
        NoVoices = 4,
        VoiceNotFound = 5,
        SpeakFailure = 6,
        MemoryFailure = 7,
        RangeOutOfBounds = 8,
        Internal = 9,
        NotSpeaking = 10,
        NotPaused = 11,
        AlreadyPaused = 12,
        InvalidUtf8 = 13,
        InvalidOperation = 14,
        AlreadyInitialized = 15,
        BackendNotAvailable = 16,
        Unknown = 17,
        InvalidAudioFormat = 18,
        InternalBackendLimitExceeded = 19,
        BackendEnteredUndefinedState = 20,
        LibraryLoadFailed = 21,
        LibraryInvalid = 22,
        IncompatibleAbi = 23,
        Count = 24
    }
}