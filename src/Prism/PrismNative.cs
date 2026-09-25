// SPDX-License-Identifier: MPL-2.0

#pragma warning disable CS0649
#pragma warning disable CS0169

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Prism.Native
{
    public static class PrismNativeLib
    {
        private const string DllName = "prism";

        public const uint PRISM_OK = 0;
        public const uint PRISM_ERROR_NOT_INITIALIZED = 1;
        public const uint PRISM_ERROR_INVALID_PARAM = 2;
        public const uint PRISM_ERROR_NOT_IMPLEMENTED = 3;
        public const uint PRISM_ERROR_NO_VOICES = 4;
        public const uint PRISM_ERROR_VOICE_NOT_FOUND = 5;
        public const uint PRISM_ERROR_SPEAK_FAILURE = 6;
        public const uint PRISM_ERROR_MEMORY_FAILURE = 7;
        public const uint PRISM_ERROR_RANGE_OUT_OF_BOUNDS = 8;
        public const uint PRISM_ERROR_INTERNAL = 9;
        public const uint PRISM_ERROR_NOT_SPEAKING = 10;
        public const uint PRISM_ERROR_NOT_PAUSED = 11;
        public const uint PRISM_ERROR_ALREADY_PAUSED = 12;
        public const uint PRISM_ERROR_INVALID_UTF8 = 13;
        public const uint PRISM_ERROR_INVALID_OPERATION = 14;
        public const uint PRISM_ERROR_ALREADY_INITIALIZED = 15;
        public const uint PRISM_ERROR_BACKEND_NOT_AVAILABLE = 16;
        public const uint PRISM_ERROR_UNKNOWN = 17;
        public const uint PRISM_ERROR_INVALID_AUDIO_FORMAT = 18;
        public const uint PRISM_ERROR_INTERNAL_BACKEND_LIMIT_EXCEEDED = 19;
        public const uint PRISM_ERROR_BACKEND_ENTERED_UNDEFINED_STATE = 20;
        public const uint PRISM_ERROR_LIBRARY_LOAD_FAILED = 21;
        public const uint PRISM_ERROR_LIBRARY_INVALID = 22;
        public const uint PRISM_ERROR_INCOMPATIBLE_ABI = 23;
        public const uint PRISM_ERROR_COUNT = 24;

        public const ulong PRISM_BACKEND_INVALID = 0;
        public const ulong PRISM_BACKEND_SAPI = 0x1D6DF72422CEEE66;
        public const ulong PRISM_BACKEND_AV_SPEECH = 0x28E3429577805C24;
        public const ulong PRISM_BACKEND_VOICE_OVER = 0xCB4897961A754BCB;
        public const ulong PRISM_BACKEND_SPEECH_DISPATCHER = 0xE3D6F895D949EBFE;
        public const ulong PRISM_BACKEND_NVDA = 0x89CC19C5C4AC1A56;
        public const ulong PRISM_BACKEND_JAWS = 0xAC3D60E9BD84B53E;
        public const ulong PRISM_BACKEND_ONE_CORE = 0x6797D32F0D994CB4;
        public const ulong PRISM_BACKEND_ORCA = 0x10AA1FC05A17F96C;
        public const ulong PRISM_BACKEND_ANDROID_SCREEN_READER = 0xD199C175AEEC494B;
        public const ulong PRISM_BACKEND_ANDROID_TTS = 0xBC175831BFE4E5CC;
        public const ulong PRISM_BACKEND_WEB_SPEECH = 0x3572538D44D44A8F;
        public const ulong PRISM_BACKEND_UIA = 0x6238F019DB678F8E;
        public const ulong PRISM_BACKEND_ZDSR = 0x3D93C56C9E7F2A2E;
        public const ulong PRISM_BACKEND_ZOOM_TEXT = 0xAE439D62DC7B1479;
        public const ulong PRISM_BACKEND_BOY_PC_READER = 0x285aba1c16f3300f;
        public const ulong PRISM_BACKEND_PC_TALKER = 0x344B951962E3B835;
        public const ulong PRISM_BACKEND_SYSTEM_ACCESS = 0x8380F2A37B2C3EB6;
        public const ulong PRISM_BACKEND_WINDOW_EYES = 0x9120D89908785C13;
        public const ulong PRISM_BACKEND_SENSE_READER = 0xED4760890B55C2F2;
        public const ulong PRISM_BACKEND_SPIEL = 0x478B44F14AD3D89C;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AvailabilityCallbackDelegate(
            IntPtr userdata,
            ulong backend,
            IntPtr name,
            [MarshalAs(UnmanagedType.I1)] bool available);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AvailabilityBaselineCallbackDelegate(IntPtr userdata);

        [StructLayout(LayoutKind.Sequential)]
        public struct PrismConfigInternal
        {
            public byte version;
            public IntPtr registry;
            public IntPtr availabilityCallback;
            public IntPtr availabilityUserdata;
            public uint availabilityPollIntervalMs;
            public uint availabilityDebounceSamples;
            public uint availabilityBackoffMaxMs;
            // The native field is a 1-byte C bool. It is stored as a byte so
            // the struct stays fully blittable: .NET Framework cannot return a
            // struct containing a bool by value (MarshalDirectiveException),
            // which prism_config_init() does.
            public byte availabilityAutoPowerManage;
            public IntPtr availabilityBaselineCallback;
        }

        public const byte PRISM_CONFIG_VERSION = 4;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AudioCallbackDelegate(
            IntPtr userdata,
            IntPtr samples,
            UIntPtr sample_count,
            UIntPtr channels,
            UIntPtr sample_rate);

        public static IntPtr StringToUtf8Ptr(string s)
        {
            if (s == null) return IntPtr.Zero;
            if (s.Length == 0)
            {
                var empty = Marshal.AllocHGlobal(1);
                Marshal.WriteByte(empty, 0, 0);
                return empty;
            }
            var bytes = Encoding.UTF8.GetBytes(s);
            var ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }

        public static void FreeUtf8Ptr(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
        }

        public static string Utf8PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return string.Empty;
            var length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
            {
                length++;
            }
            if (length == 0) return string.Empty;
            var bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern PrismConfigInternal prism_config_init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_init(ref PrismConfigInternal cfg);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_shutdown(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_availability_poll_pause(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_availability_poll_resume(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool prism_availability_auto_power_supported();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern UIntPtr prism_registry_count(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern ulong prism_registry_id_at(IntPtr ctx, UIntPtr index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern ulong prism_registry_id(IntPtr ctx, IntPtr name);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_name(IntPtr ctx, ulong id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int prism_registry_priority(IntPtr ctx, ulong id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool prism_registry_exists(IntPtr ctx, ulong id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_get(IntPtr ctx, ulong id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_create(IntPtr ctx, ulong id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_create_best(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_acquire(IntPtr ctx, ulong id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_acquire_best(IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_backend_free(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_backend_name(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern ulong prism_backend_get_features(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_initialize(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_speak(IntPtr backend, IntPtr text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_speak_to_memory(
            IntPtr backend,
            IntPtr text,
            AudioCallbackDelegate callback,
            IntPtr userdata);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_braille(IntPtr backend, IntPtr text);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_output(IntPtr backend, IntPtr text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_stop(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_pause(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_resume(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_is_speaking(IntPtr backend, [MarshalAs(UnmanagedType.I1)] out bool out_speaking);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_set_volume(IntPtr backend, float volume);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_set_rate(IntPtr backend, float rate);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_set_pitch(IntPtr backend, float pitch);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_volume(IntPtr backend, out float out_volume);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_rate(IntPtr backend, out float out_rate);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_pitch(IntPtr backend, out float out_pitch);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_refresh_voices(IntPtr backend);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_count_voices(IntPtr backend, out UIntPtr out_count);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_voice_name(IntPtr backend, UIntPtr voice_id, out IntPtr out_name);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_voice_language(IntPtr backend, UIntPtr voice_id, out IntPtr out_language);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_set_voice(IntPtr backend, UIntPtr voice_id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_voice(IntPtr backend, out UIntPtr out_voice_id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_channels(IntPtr backend, out UIntPtr out_channels);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_sample_rate(IntPtr backend, out UIntPtr out_sample_rate);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_backend_get_bit_depth(IntPtr backend, out UIntPtr out_bit_depth);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_error_string(uint error);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_version();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_version_string();

        public const ulong PRISM_PLUGIN_ABI_VERSION = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct PrismBackendVTableInternal
        {
            public UIntPtr size;
            public IntPtr create;
            public IntPtr destroy;
            public IntPtr is_supported;
            public IntPtr initialize;
            public IntPtr speak;
            public IntPtr speak_to_memory;
            public IntPtr braille;
            public IntPtr output;
            public IntPtr stop;
            public IntPtr pause;
            public IntPtr resume;
            public IntPtr is_speaking;
            public IntPtr set_volume;
            public IntPtr get_volume;
            public IntPtr set_rate;
            public IntPtr get_rate;
            public IntPtr set_pitch;
            public IntPtr get_pitch;
            public IntPtr refresh_voices;
            public IntPtr count_voices;
            public IntPtr get_voice_name;
            public IntPtr get_voice_language;
            public IntPtr set_voice;
            public IntPtr get_voice;
            public IntPtr get_channels;
            public IntPtr get_sample_rate;
            public IntPtr get_bit_depth;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_builder_new();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_registry_builder_add_backend(
            IntPtr builder,
            IntPtr name,
            int priority,
            ulong features,
            ref PrismBackendVTableInternal vtable,
            IntPtr userdata,
            IntPtr userdata_free,
            out ulong out_id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint prism_registry_builder_add_library(
            IntPtr builder,
            IntPtr path,
            int priority_override,
            out UIntPtr out_count);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_freeze(IntPtr builder);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_registry_builder_free(IntPtr builder);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr prism_registry_retain(IntPtr registry);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_registry_release(IntPtr registry);

        public const int PRISM_LOG_LEVEL_TRACE = 0;
        public const int PRISM_LOG_LEVEL_DEBUG = 1;
        public const int PRISM_LOG_LEVEL_INFO = 2;
        public const int PRISM_LOG_LEVEL_WARN = 3;
        public const int PRISM_LOG_LEVEL_ERROR = 4;
        public const int PRISM_LOG_LEVEL_NONE = 5;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LogCallbackDelegate(
            IntPtr userdata,
            int level,
            IntPtr source,
            IntPtr message);

        [StructLayout(LayoutKind.Sequential)]
        public struct PrismLogHandlerInternal
        {
            public IntPtr fn;
            public IntPtr userdata;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern PrismLogHandlerInternal prism_set_log_handler(PrismLogHandlerInternal handler);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int prism_set_log_level(int level);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_log(int level, IntPtr source, IntPtr message);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_log_flush();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void prism_log_shutdown();
    }
}