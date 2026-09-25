// SPDX-License-Identifier: MPL-2.0

using System;
using Prism.Primitives;

namespace Prism.Exceptions
{
    public class PrismException : Exception
    {
        public PrismError Code { get; }

        public PrismException(PrismError code, string? message) : base(message)
        {
            Code = code;
        }

        public PrismException(PrismError code) : this(code, $"Prism error: {code}")
        {
        }
    }

    public class PrismNotInitializedException : PrismException
    {
        public PrismNotInitializedException() : base(PrismError.NotInitialized, "Prism is not initialized") { }
        public PrismNotInitializedException(string? message) : base(PrismError.NotInitialized, message) { }
    }

    public class PrismInvalidParamException : PrismException
    {
        public PrismInvalidParamException() : base(PrismError.InvalidParam, "Invalid parameter") { }
        public PrismInvalidParamException(string? message) : base(PrismError.InvalidParam, message) { }
    }

    public class PrismNotImplementedException : PrismException
    {
        public PrismNotImplementedException() : base(PrismError.NotImplemented, "Operation not implemented") { }
        public PrismNotImplementedException(string? message) : base(PrismError.NotImplemented, message) { }
    }

    public class PrismNoVoicesException : PrismException
    {
        public PrismNoVoicesException() : base(PrismError.NoVoices, "No voices available") { }
        public PrismNoVoicesException(string? message) : base(PrismError.NoVoices, message) { }
    }

    public class PrismVoiceNotFoundException : PrismException
    {
        public PrismVoiceNotFoundException() : base(PrismError.VoiceNotFound, "Voice not found") { }
        public PrismVoiceNotFoundException(string? message) : base(PrismError.VoiceNotFound, message) { }
    }

    public class PrismSpeakException : PrismException
    {
        public PrismSpeakException() : base(PrismError.SpeakFailure, "Speech synthesis failed") { }
        public PrismSpeakException(string? message) : base(PrismError.SpeakFailure, message) { }
    }

    public class PrismMemoryException : PrismException
    {
        public PrismMemoryException() : base(PrismError.MemoryFailure, "Memory allocation failed") { }
        public PrismMemoryException(string? message) : base(PrismError.MemoryFailure, message) { }
    }

    public class PrismRangeException : PrismException
    {
        public PrismRangeException() : base(PrismError.RangeOutOfBounds, "Value out of range") { }
        public PrismRangeException(string? message) : base(PrismError.RangeOutOfBounds, message) { }
    }

    public class PrismInternalException : PrismException
    {
        public PrismInternalException() : base(PrismError.Internal, "Internal error") { }
        public PrismInternalException(string? message) : base(PrismError.Internal, message) { }
    }

    public class PrismNotSpeakingException : PrismException
    {
        public PrismNotSpeakingException() : base(PrismError.NotSpeaking, "Not currently speaking") { }
        public PrismNotSpeakingException(string? message) : base(PrismError.NotSpeaking, message) { }
    }

    public class PrismNotPausedException : PrismException
    {
        public PrismNotPausedException() : base(PrismError.NotPaused, "Not currently paused") { }
        public PrismNotPausedException(string? message) : base(PrismError.NotPaused, message) { }
    }

    public class PrismAlreadyPausedException : PrismException
    {
        public PrismAlreadyPausedException() : base(PrismError.AlreadyPaused, "Already paused") { }
        public PrismAlreadyPausedException(string? message) : base(PrismError.AlreadyPaused, message) { }
    }

    public class PrismInvalidUtf8Exception : PrismException
    {
        public PrismInvalidUtf8Exception() : base(PrismError.InvalidUtf8, "Invalid UTF-8 string") { }
        public PrismInvalidUtf8Exception(string? message) : base(PrismError.InvalidUtf8, message) { }
    }

    public class PrismInvalidOperationException : PrismException
    {
        public PrismInvalidOperationException() : base(PrismError.InvalidOperation, "Invalid operation") { }
        public PrismInvalidOperationException(string? message) : base(PrismError.InvalidOperation, message) { }
    }

    public class PrismAlreadyInitializedException : PrismException
    {
        public PrismAlreadyInitializedException() : base(PrismError.AlreadyInitialized, "Already initialized") { }
        public PrismAlreadyInitializedException(string? message) : base(PrismError.AlreadyInitialized, message) { }
    }

    public class PrismBackendNotAvailableException : PrismException
    {
        public PrismBackendNotAvailableException() : base(PrismError.BackendNotAvailable, "Backend not available") { }
        public PrismBackendNotAvailableException(string? message) : base(PrismError.BackendNotAvailable, message) { }
    }

    public class PrismUnknownException : PrismException
    {
        public PrismUnknownException() : base(PrismError.Unknown, "Unknown error") { }
        public PrismUnknownException(string? message) : base(PrismError.Unknown, message) { }
    }

    public class PrismInvalidAudioFormatException : PrismException
    {
        public PrismInvalidAudioFormatException() : base(PrismError.InvalidAudioFormat, "Invalid audio format") { }
        public PrismInvalidAudioFormatException(string? message) : base(PrismError.InvalidAudioFormat, message) { }
    }

    public class PrismInternalBackendLimitExceededException : PrismException
    {
        public PrismInternalBackendLimitExceededException() : base(PrismError.InternalBackendLimitExceeded, "Backend limit exceeded") { }
        public PrismInternalBackendLimitExceededException(string? message) : base(PrismError.InternalBackendLimitExceeded, message) { }
    }

    public class PrismBackendEnteredUndefinedStateException : PrismException
    {
        public PrismBackendEnteredUndefinedStateException() : base(PrismError.BackendEnteredUndefinedState, "Backend entered undefined state") { }
        public PrismBackendEnteredUndefinedStateException(string? message) : base(PrismError.BackendEnteredUndefinedState, message) { }
    }

    public class PrismLibraryLoadFailedException : PrismException
    {
        public PrismLibraryLoadFailedException() : base(PrismError.LibraryLoadFailed, "Shared library load failed") { }
        public PrismLibraryLoadFailedException(string? message) : base(PrismError.LibraryLoadFailed, message) { }
    }

    public class PrismLibraryInvalidException : PrismException
    {
        public PrismLibraryInvalidException() : base(PrismError.LibraryInvalid, "Shared library is not a Prism plugin") { }
        public PrismLibraryInvalidException(string? message) : base(PrismError.LibraryInvalid, message) { }
    }

    public class PrismIncompatibleAbiException : PrismException
    {
        public PrismIncompatibleAbiException() : base(PrismError.IncompatibleAbi, "Incompatible plugin ABI") { }
        public PrismIncompatibleAbiException(string? message) : base(PrismError.IncompatibleAbi, message) { }
    }

    public class PrismInitializationException : PrismException
    {
        public PrismInitializationException() : base(PrismError.Unknown, "Prism initialization failed") { }
        public PrismInitializationException(string? message) : base(PrismError.Unknown, message) { }
    }
}