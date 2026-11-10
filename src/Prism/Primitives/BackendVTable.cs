// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Prism.Primitives
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr BackendCreateDelegate(IntPtr userdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void BackendDestroyDelegate(IntPtr instance);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public delegate bool BackendIsSupportedDelegate(IntPtr instance);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendInitializeDelegate(IntPtr instance);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendSpeakDelegate(IntPtr instance, IntPtr text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendSpeakToMemoryDelegate(IntPtr instance, IntPtr text, PrismNativeLib.AudioCallbackDelegate callback, IntPtr callbackUserdata);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendBrailleDelegate(IntPtr instance, IntPtr text);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendOutputDelegate(IntPtr instance, IntPtr text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendSimpleDelegate(IntPtr instance);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendIsSpeakingDelegate(IntPtr instance, [MarshalAs(UnmanagedType.I1)] out bool outSpeaking);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendSetFloatDelegate(IntPtr instance, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendGetFloatDelegate(IntPtr instance, out float outValue);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendCountVoicesDelegate(IntPtr instance, out UIntPtr outCount);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendGetVoiceStringDelegate(IntPtr instance, UIntPtr voiceId, out IntPtr outValue);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendSetVoiceDelegate(IntPtr instance, UIntPtr voiceId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendGetVoiceDelegate(IntPtr instance, out UIntPtr outVoiceId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint BackendGetSizeDelegate(IntPtr instance, out UIntPtr outValue);

    /// <summary>
    /// Creates reverse P/Invoke thunks for custom-backend callbacks. Managed
    /// exceptions must not escape into native Prism, so every thunk catches
    /// them and returns a native-safe failure value.
    /// </summary>
    internal static class BackendThunks
    {
        public static BackendCreateDelegate Wrap(BackendCreateDelegate inner) =>
            userdata =>
            {
                try
                {
                    return inner(userdata);
                }
                catch (Exception)
                {
                    return IntPtr.Zero;
                }
            };

        public static BackendDestroyDelegate Wrap(BackendDestroyDelegate inner) =>
            instance =>
            {
                try
                {
                    inner(instance);
                }
                catch (Exception)
                {
                    // Destruction is best effort; no result can be reported.
                }
            };

        public static BackendIsSupportedDelegate Wrap(BackendIsSupportedDelegate inner) =>
            instance =>
            {
                try
                {
                    return inner(instance);
                }
                catch (Exception)
                {
                    return false;
                }
            };

        public static BackendInitializeDelegate Wrap(BackendInitializeDelegate inner) =>
            instance =>
            {
                try
                {
                    return inner(instance);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendSpeakDelegate Wrap(BackendSpeakDelegate inner) =>
            (instance, text, interrupt) =>
            {
                try
                {
                    return inner(instance, text, interrupt);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendSpeakToMemoryDelegate Wrap(BackendSpeakToMemoryDelegate inner) =>
            (instance, text, callback, callbackUserdata) =>
            {
                try
                {
                    return inner(instance, text, callback, callbackUserdata);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendBrailleDelegate Wrap(BackendBrailleDelegate inner) =>
            (instance, text) =>
            {
                try
                {
                    return inner(instance, text);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendOutputDelegate Wrap(BackendOutputDelegate inner) =>
            (instance, text, interrupt) =>
            {
                try
                {
                    return inner(instance, text, interrupt);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendSimpleDelegate Wrap(BackendSimpleDelegate inner) =>
            instance =>
            {
                try
                {
                    return inner(instance);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendIsSpeakingDelegate Wrap(BackendIsSpeakingDelegate inner) =>
            (IntPtr instance, out bool outSpeaking) =>
            {
                try
                {
                    return inner(instance, out outSpeaking);
                }
                catch (Exception)
                {
                    outSpeaking = false;
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendSetFloatDelegate Wrap(BackendSetFloatDelegate inner) =>
            (instance, value) =>
            {
                try
                {
                    return inner(instance, value);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendGetFloatDelegate Wrap(BackendGetFloatDelegate inner) =>
            (IntPtr instance, out float outValue) =>
            {
                try
                {
                    return inner(instance, out outValue);
                }
                catch (Exception)
                {
                    outValue = 0;
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendCountVoicesDelegate Wrap(BackendCountVoicesDelegate inner) =>
            (IntPtr instance, out UIntPtr outCount) =>
            {
                try
                {
                    return inner(instance, out outCount);
                }
                catch (Exception)
                {
                    outCount = UIntPtr.Zero;
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendGetVoiceStringDelegate Wrap(BackendGetVoiceStringDelegate inner) =>
            (IntPtr instance, UIntPtr voiceId, out IntPtr outValue) =>
            {
                try
                {
                    return inner(instance, voiceId, out outValue);
                }
                catch (Exception)
                {
                    outValue = IntPtr.Zero;
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendSetVoiceDelegate Wrap(BackendSetVoiceDelegate inner) =>
            (instance, voiceId) =>
            {
                try
                {
                    return inner(instance, voiceId);
                }
                catch (Exception)
                {
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendGetVoiceDelegate Wrap(BackendGetVoiceDelegate inner) =>
            (IntPtr instance, out UIntPtr outVoiceId) =>
            {
                try
                {
                    return inner(instance, out outVoiceId);
                }
                catch (Exception)
                {
                    outVoiceId = UIntPtr.Zero;
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };

        public static BackendGetSizeDelegate Wrap(BackendGetSizeDelegate inner) =>
            (IntPtr instance, out UIntPtr outValue) =>
            {
                try
                {
                    return inner(instance, out outValue);
                }
                catch (Exception)
                {
                    outValue = UIntPtr.Zero;
                    return PrismNativeLib.PRISM_ERROR_INTERNAL;
                }
            };
    }

    /// <summary>
    /// Managed representation of a custom-backend vtable. Null members are
    /// reported as absent to Prism; the corresponding <see cref="BackendFeatures"/>
    /// bits must not be declared for absent members.
    /// </summary>
    public sealed class BackendVTable
    {
        public BackendCreateDelegate? Create { get; set; }
        public BackendDestroyDelegate? Destroy { get; set; }
        public BackendIsSupportedDelegate? IsSupported { get; set; }
        public BackendInitializeDelegate? Initialize { get; set; }
        public BackendSpeakDelegate? Speak { get; set; }
        public BackendSpeakToMemoryDelegate? SpeakToMemory { get; set; }
        public BackendBrailleDelegate? Braille { get; set; }
        public BackendOutputDelegate? Output { get; set; }
        public BackendSimpleDelegate? Stop { get; set; }
        public BackendSimpleDelegate? Pause { get; set; }
        public BackendSimpleDelegate? Resume { get; set; }
        public BackendIsSpeakingDelegate? IsSpeaking { get; set; }
        public BackendSetFloatDelegate? SetVolume { get; set; }
        public BackendGetFloatDelegate? GetVolume { get; set; }
        public BackendSetFloatDelegate? SetRate { get; set; }
        public BackendGetFloatDelegate? GetRate { get; set; }
        public BackendSetFloatDelegate? SetPitch { get; set; }
        public BackendGetFloatDelegate? GetPitch { get; set; }
        public BackendSimpleDelegate? RefreshVoices { get; set; }
        public BackendCountVoicesDelegate? CountVoices { get; set; }
        public BackendGetVoiceStringDelegate? GetVoiceName { get; set; }
        public BackendGetVoiceStringDelegate? GetVoiceLanguage { get; set; }
        public BackendSetVoiceDelegate? SetVoice { get; set; }
        public BackendGetVoiceDelegate? GetVoice { get; set; }
        public BackendGetSizeDelegate? GetChannels { get; set; }
        public BackendGetSizeDelegate? GetSampleRate { get; set; }
        public BackendGetSizeDelegate? GetBitDepth { get; set; }

        internal PrismNativeLib.PrismBackendVTableInternal ToNative(List<Delegate> thunks)
        {
            IntPtr Pin<TDelegate>(TDelegate thunk)
                where TDelegate : Delegate
            {
                thunks.Add(thunk);
                return Marshal.GetFunctionPointerForDelegate(thunk);
            }

            return new PrismNativeLib.PrismBackendVTableInternal
            {
                size = (UIntPtr)Marshal.SizeOf<PrismNativeLib.PrismBackendVTableInternal>(),
                create = Create == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Create)),
                destroy = Destroy == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Destroy)),
                is_supported = IsSupported == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(IsSupported)),
                initialize = Initialize == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Initialize)),
                speak = Speak == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Speak)),
                speak_to_memory = SpeakToMemory == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(SpeakToMemory)),
                braille = Braille == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Braille)),
                output = Output == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Output)),
                stop = Stop == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Stop)),
                pause = Pause == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Pause)),
                resume = Resume == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(Resume)),
                is_speaking = IsSpeaking == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(IsSpeaking)),
                set_volume = SetVolume == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(SetVolume)),
                get_volume = GetVolume == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetVolume)),
                set_rate = SetRate == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(SetRate)),
                get_rate = GetRate == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetRate)),
                set_pitch = SetPitch == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(SetPitch)),
                get_pitch = GetPitch == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetPitch)),
                refresh_voices = RefreshVoices == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(RefreshVoices)),
                count_voices = CountVoices == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(CountVoices)),
                get_voice_name = GetVoiceName == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetVoiceName)),
                get_voice_language = GetVoiceLanguage == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetVoiceLanguage)),
                set_voice = SetVoice == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(SetVoice)),
                get_voice = GetVoice == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetVoice)),
                get_channels = GetChannels == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetChannels)),
                get_sample_rate = GetSampleRate == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetSampleRate)),
                get_bit_depth = GetBitDepth == null ? IntPtr.Zero : Pin(BackendThunks.Wrap(GetBitDepth))
            };
        }
    }
}
