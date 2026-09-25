// SPDX-License-Identifier: MPL-2.0

using System;
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

        internal PrismNativeLib.PrismBackendVTableInternal ToNative()
        {
            return new PrismNativeLib.PrismBackendVTableInternal
            {
                size = (UIntPtr)Marshal.SizeOf<PrismNativeLib.PrismBackendVTableInternal>(),
                create = Create == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Create),
                destroy = Destroy == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Destroy),
                is_supported = IsSupported == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(IsSupported),
                initialize = Initialize == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Initialize),
                speak = Speak == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Speak),
                speak_to_memory = SpeakToMemory == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(SpeakToMemory),
                braille = Braille == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Braille),
                output = Output == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Output),
                stop = Stop == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Stop),
                pause = Pause == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Pause),
                resume = Resume == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(Resume),
                is_speaking = IsSpeaking == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(IsSpeaking),
                set_volume = SetVolume == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(SetVolume),
                get_volume = GetVolume == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetVolume),
                set_rate = SetRate == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(SetRate),
                get_rate = GetRate == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetRate),
                set_pitch = SetPitch == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(SetPitch),
                get_pitch = GetPitch == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetPitch),
                refresh_voices = RefreshVoices == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(RefreshVoices),
                count_voices = CountVoices == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(CountVoices),
                get_voice_name = GetVoiceName == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetVoiceName),
                get_voice_language = GetVoiceLanguage == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetVoiceLanguage),
                set_voice = SetVoice == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(SetVoice),
                get_voice = GetVoice == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetVoice),
                get_channels = GetChannels == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetChannels),
                get_sample_rate = GetSampleRate == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetSampleRate),
                get_bit_depth = GetBitDepth == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(GetBitDepth)
            };
        }
    }
}
