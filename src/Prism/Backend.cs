// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prism.Primitives;
using Prism.Exceptions;

namespace Prism
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    public sealed class Backend : IDisposable
    {
        private IntPtr _backend;
        private bool _disposed;
        private readonly Context _owner;
        private readonly object _audioCallbackGate = new object();
        private readonly List<PrismNativeLib.AudioCallbackDelegate> _audioCallbacks = new List<PrismNativeLib.AudioCallbackDelegate>();

        internal Backend(Context owner, IntPtr handle)
        {
            _owner = owner;
            _backend = handle;
            var result = PrismNativeLib.prism_backend_initialize(_backend);
            if (result != PrismNativeLib.PRISM_OK && result != PrismNativeLib.PRISM_ERROR_ALREADY_INITIALIZED)
            {
                PrismNativeLib.prism_backend_free(_backend);
                _backend = IntPtr.Zero;
                throw CreateException(result);
            }
            owner?.RegisterBackend(this);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                var handle = _backend;
                _backend = IntPtr.Zero;
                _disposed = true;
                lock (_audioCallbackGate)
                {
                    _audioCallbacks.Clear();
                }
                if (handle != IntPtr.Zero)
                {
                    PrismNativeLib.prism_backend_free(handle);
                }
                _owner?.UnregisterBackend(this);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new PrismInvalidOperationException("Backend has been disposed");
            }
        }

        internal static Exception CreateException(uint errorCode)
        {
            var prismError = (PrismError)errorCode;
            return prismError switch
            {
                PrismError.NotInitialized => new PrismNotInitializedException(),
                PrismError.InvalidParam => new PrismInvalidParamException(),
                PrismError.NotImplemented => new PrismNotImplementedException(),
                PrismError.NoVoices => new PrismNoVoicesException(),
                PrismError.VoiceNotFound => new PrismVoiceNotFoundException(),
                PrismError.SpeakFailure => new PrismSpeakException(),
                PrismError.MemoryFailure => new PrismMemoryException(),
                PrismError.RangeOutOfBounds => new PrismRangeException(),
                PrismError.Internal => new PrismInternalException(),
                PrismError.NotSpeaking => new PrismNotSpeakingException(),
                PrismError.NotPaused => new PrismNotPausedException(),
                PrismError.AlreadyPaused => new PrismAlreadyPausedException(),
                PrismError.InvalidUtf8 => new PrismInvalidUtf8Exception(),
                PrismError.InvalidOperation => new PrismInvalidOperationException(),
                PrismError.AlreadyInitialized => new PrismAlreadyInitializedException(),
                PrismError.BackendNotAvailable => new PrismBackendNotAvailableException(),
                PrismError.Unknown => new PrismUnknownException(),
                PrismError.InvalidAudioFormat => new PrismInvalidAudioFormatException(),
                PrismError.InternalBackendLimitExceeded => new PrismInternalBackendLimitExceededException(),
                PrismError.BackendEnteredUndefinedState => new PrismBackendEnteredUndefinedStateException(),
                PrismError.LibraryLoadFailed => new PrismLibraryLoadFailedException(),
                PrismError.LibraryInvalid => new PrismLibraryInvalidException(),
                PrismError.IncompatibleAbi => new PrismIncompatibleAbiException(),
                _ => new PrismUnknownException($"Unknown error code: {errorCode}")
            };
        }

        public string Name
        {
            get
            {
                ThrowIfDisposed();
                var ptr = PrismNativeLib.prism_backend_name(_backend);
                return ptr == IntPtr.Zero ? string.Empty : PrismNativeLib.Utf8PtrToString(ptr);
            }
        }

        public BackendFeatures Features
        {
            get
            {
                ThrowIfDisposed();
                return (BackendFeatures)PrismNativeLib.prism_backend_get_features(_backend);
            }
        }

        public void Speak(string text, bool interrupt = false)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            var textPtr = PrismNativeLib.StringToUtf8Ptr(text);
            try
            {
                var result = PrismNativeLib.prism_backend_speak(_backend, textPtr, interrupt);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(textPtr);
            }
        }

        public void SpeakToMemory(string text, Action<float[], int, int> onAudioData)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            if (onAudioData == null)
            {
                throw new ArgumentNullException(nameof(onAudioData));
            }

            PrismNativeLib.AudioCallbackDelegate callback = (IntPtr userdata, IntPtr samples, UIntPtr sampleCount, UIntPtr channels, UIntPtr sampleRate) =>
            {
                try
                {
                    var sampleCountValue = (ulong)sampleCount;
                    var channelsValue = (ulong)channels;
                    var sampleRateValue = (ulong)sampleRate;
                    long totalSamples = sampleCountValue > (ulong)long.MaxValue
                        ? long.MaxValue
                        : (long)sampleCountValue;
                    if (totalSamples <= 0 || samples == IntPtr.Zero ||
                        sampleCountValue > int.MaxValue ||
                        channelsValue > int.MaxValue ||
                        sampleRateValue > int.MaxValue)
                    {
                        return;
                    }
                    var floatArray = new float[(int)totalSamples];
                    Marshal.Copy(samples, floatArray, 0, (int)totalSamples);
                    onAudioData(floatArray, (int)channelsValue, (int)sampleRateValue);
                }
                catch
                {
                    // The audio callback MUST NOT throw across the native
                    // boundary (undefined behavior). Swallow user errors.
                }
            };

            // Keep the delegate rooted while native code may invoke it.
            lock (_audioCallbackGate)
            {
                _audioCallbacks.Add(callback);
            }

            IntPtr textPtr = IntPtr.Zero;
            try
            {
                textPtr = PrismNativeLib.StringToUtf8Ptr(text);
                var result = PrismNativeLib.prism_backend_speak_to_memory(_backend, textPtr, callback, IntPtr.Zero);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(textPtr);
                lock (_audioCallbackGate)
                {
                    _audioCallbacks.Remove(callback);
                }
            }
        }

        public void Braille(string text)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            var textPtr = PrismNativeLib.StringToUtf8Ptr(text);
            try
            {
                var result = PrismNativeLib.prism_backend_braille(_backend, textPtr);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(textPtr);
            }
        }

        public void Output(string text, bool interrupt = false)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            var textPtr = PrismNativeLib.StringToUtf8Ptr(text);
            try
            {
                var result = PrismNativeLib.prism_backend_output(_backend, textPtr, interrupt);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(textPtr);
            }
        }

        public void Stop()
        {
            ThrowIfDisposed();
            var result = PrismNativeLib.prism_backend_stop(_backend);
            if (result != PrismNativeLib.PRISM_OK)
            {
                throw CreateException(result);
            }
        }

        public void Pause()
        {
            ThrowIfDisposed();
            var result = PrismNativeLib.prism_backend_pause(_backend);
            if (result != PrismNativeLib.PRISM_OK)
            {
                throw CreateException(result);
            }
        }

        public void Resume()
        {
            ThrowIfDisposed();
            var result = PrismNativeLib.prism_backend_resume(_backend);
            if (result != PrismNativeLib.PRISM_OK)
            {
                throw CreateException(result);
            }
        }

        public bool IsSpeaking
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_is_speaking(_backend, out bool speaking);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return speaking;
            }
        }

        public float Volume
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_get_volume(_backend, out float volume);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return volume;
            }
            set
            {
                ThrowIfDisposed();
                if (value < 0.0f || value > 1.0f || float.IsNaN(value) || float.IsInfinity(value))
                {
                    throw new PrismRangeException("Volume must be in range [0.0, 1.0]");
                }
                var result = PrismNativeLib.prism_backend_set_volume(_backend, value);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
        }

        public float Rate
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_get_rate(_backend, out float rate);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return rate;
            }
            set
            {
                ThrowIfDisposed();
                if (value < 0.0f || value > 1.0f || float.IsNaN(value) || float.IsInfinity(value))
                {
                    throw new PrismRangeException("Rate must be in range [0.0, 1.0]");
                }
                var result = PrismNativeLib.prism_backend_set_rate(_backend, value);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
        }

        public float Pitch
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_get_pitch(_backend, out float pitch);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return pitch;
            }
            set
            {
                ThrowIfDisposed();
                if (value < 0.0f || value > 1.0f || float.IsNaN(value) || float.IsInfinity(value))
                {
                    throw new PrismRangeException("Pitch must be in range [0.0, 1.0]");
                }
                var result = PrismNativeLib.prism_backend_set_pitch(_backend, value);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
        }

        public void RefreshVoices()
        {
            ThrowIfDisposed();
            var result = PrismNativeLib.prism_backend_refresh_voices(_backend);
            if (result != PrismNativeLib.PRISM_OK)
            {
                throw CreateException(result);
            }
        }

        public int VoicesCount
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_count_voices(_backend, out UIntPtr count);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return ToInt32(count, "Voice count");
            }
        }

        public string GetVoiceName(int voiceId)
        {
            ThrowIfDisposed();
            if (voiceId < 0)
            {
                throw new PrismRangeException("Voice ID must be non-negative");
            }
            var result = PrismNativeLib.prism_backend_get_voice_name(_backend, (UIntPtr)voiceId, out IntPtr name);
            if (result != PrismNativeLib.PRISM_OK)
            {
                throw CreateException(result);
            }
            return name == IntPtr.Zero ? string.Empty : PrismNativeLib.Utf8PtrToString(name);
        }

        public string GetVoiceLanguage(int voiceId)
        {
            ThrowIfDisposed();
            if (voiceId < 0)
            {
                throw new PrismRangeException("Voice ID must be non-negative");
            }
            var result = PrismNativeLib.prism_backend_get_voice_language(_backend, (UIntPtr)voiceId, out IntPtr language);
            if (result != PrismNativeLib.PRISM_OK)
            {
                throw CreateException(result);
            }
            return language == IntPtr.Zero ? string.Empty : PrismNativeLib.Utf8PtrToString(language);
        }

        public int Voice
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_get_voice(_backend, out UIntPtr voiceId);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return ToInt32(voiceId, "Voice ID");
            }
            set
            {
                ThrowIfDisposed();
                if (value < 0)
                {
                    throw new PrismRangeException("Voice ID must be non-negative");
                }
                var result = PrismNativeLib.prism_backend_set_voice(_backend, (UIntPtr)value);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
            }
        }

        public int Channels
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_get_channels(_backend, out UIntPtr channels);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return ToInt32(channels, "Channel count");
            }
        }

        public int SampleRate
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_get_sample_rate(_backend, out UIntPtr sampleRate);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return ToInt32(sampleRate, "Sample rate");
            }
        }

        public int BitDepth
        {
            get
            {
                ThrowIfDisposed();
                var result = PrismNativeLib.prism_backend_get_bit_depth(_backend, out UIntPtr bitDepth);
                if (result != PrismNativeLib.PRISM_OK)
                {
                    throw CreateException(result);
                }
                return ToInt32(bitDepth, "Bit depth");
            }
        }

        private static int ToInt32(UIntPtr value, string name)
        {
            var number = value.ToUInt64();
            if (number > int.MaxValue)
            {
                throw new PrismRangeException($"{name} exceeds Int32.MaxValue");
            }
            return (int)number;
        }
    }
}
