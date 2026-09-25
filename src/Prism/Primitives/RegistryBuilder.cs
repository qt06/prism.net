// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Prism.Exceptions;
using Prism.Native;

namespace Prism.Primitives
{
    using PrismNativeLib = Prism.Native.PrismNativeLib;

    /// <summary>
    /// Builder for a custom Prism registry. A builder is live until it is
    /// successfully frozen; after that it is spent and must be disposed.
    /// </summary>
    public sealed class RegistryBuilder : IDisposable
    {
        private IntPtr _builder;
        private bool _disposed;
        private readonly List<Delegate> _rootedDelegates = new List<Delegate>();

        public RegistryBuilder()
        {
            _builder = PrismNativeLib.prism_registry_builder_new();
            if (_builder == IntPtr.Zero)
            {
                throw new PrismMemoryException("Failed to create Prism registry builder");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                PrismNativeLib.prism_registry_builder_free(_builder);
                _builder = IntPtr.Zero;
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new PrismInvalidOperationException("RegistryBuilder has been disposed");
            }
        }

        /// <summary>
        /// Registers a custom backend. The vtable delegates and
        /// <paramref name="userdata"/> must stay valid for as long as the
        /// registration is referenced (the frozen registry or any backend
        /// instance created from it); the builder roots the delegates
        /// internally and transfers that responsibility to the frozen
        /// <see cref="PrismRegistry"/> on <see cref="Freeze"/>.
        /// </summary>
        public BackendId AddBackend(string name, int priority, BackendFeatures features, BackendVTable vtable, IntPtr userdata = default)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Backend name must be a non-empty string", nameof(name));
            }
            if (vtable == null)
            {
                throw new ArgumentNullException(nameof(vtable));
            }
            if (priority < 0)
            {
                throw new PrismRangeException("Priority must be non-negative");
            }

            var namePtr = PrismNativeLib.StringToUtf8Ptr(name);
            var native = vtable.ToNative();
            var featureBits = (ulong)features;
            if (!IsConsistent(featureBits, native))
            {
                throw new ArgumentException(
                    "Feature declaration mismatch: every declared feature must have a non-null vtable member, " +
                    "and every non-null vtable member must have its feature declared.", nameof(features));
            }
            // Root every thunk delegate so the function pointers remain valid.
            _rootedDelegates.AddRange(CollectDelegates(vtable));
            try
            {
                var result = PrismNativeLib.prism_registry_builder_add_backend(
                    _builder, namePtr, priority, (ulong)features, ref native, userdata, IntPtr.Zero, out var outId);
                ThrowIfError(result);
                return (BackendId)outId;
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(namePtr);
            }
        }

        /// <summary>
        /// Loads backends from a Prism plugin shared library. Returns the
        /// number of backends added.
        /// </summary>
        public int AddLibrary(string path, int priorityOverride = -1)
        {
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Plugin path must be a non-empty string", nameof(path));
            }
            var pathPtr = PrismNativeLib.StringToUtf8Ptr(path);
            try
            {
                var result = PrismNativeLib.prism_registry_builder_add_library(_builder, pathPtr, priorityOverride, out var outCount);
                ThrowIfError(result);
                return (int)outCount;
            }
            finally
            {
                PrismNativeLib.FreeUtf8Ptr(pathPtr);
            }
        }

        /// <summary>
        /// Freezes the builder into an immutable registry. The builder becomes
        /// spent and must still be disposed. Ownership of the rooted custom
        /// backend delegates transfers to the returned registry, which must be
        /// kept alive while backends created from it are in use.
        /// </summary>
        public PrismRegistry Freeze()
        {
            ThrowIfDisposed();
            var registry = PrismNativeLib.prism_registry_freeze(_builder);
            if (registry == IntPtr.Zero)
            {
                throw new PrismMemoryException("Failed to freeze Prism registry");
            }
            var rooted = new List<Delegate>(_rootedDelegates);
            _rootedDelegates.Clear();
            return new PrismRegistry(registry, rooted);
        }

        private static void ThrowIfError(uint error)
        {
            if (error == PrismNativeLib.PRISM_OK)
            {
                return;
            }
            switch ((PrismError)error)
            {
                case PrismError.InvalidUtf8:
                    throw new PrismInvalidUtf8Exception();
                case PrismError.InvalidParam:
                    throw new PrismInvalidParamException();
                case PrismError.InvalidOperation:
                    throw new PrismInvalidOperationException();
                case PrismError.LibraryLoadFailed:
                    throw new PrismLibraryLoadFailedException();
                case PrismError.LibraryInvalid:
                    throw new PrismLibraryInvalidException();
                case PrismError.IncompatibleAbi:
                    throw new PrismIncompatibleAbiException();
                default:
                    throw new PrismUnknownException($"Prism error: {(PrismError)error}");
            }
        }

        private static bool IsConsistent(ulong features, PrismNativeLib.PrismBackendVTableInternal vtable)
        {
            return Has(features, BackendFeatures.SupportsSpeak, vtable.speak)
                && Has(features, BackendFeatures.SupportsSpeakToMemory, vtable.speak_to_memory)
                && Has(features, BackendFeatures.SupportsBraille, vtable.braille)
                && Has(features, BackendFeatures.SupportsOutput, vtable.output)
                && Has(features, BackendFeatures.SupportsIsSpeaking, vtable.is_speaking)
                && Has(features, BackendFeatures.SupportsStop, vtable.stop)
                && Has(features, BackendFeatures.SupportsPause, vtable.pause)
                && Has(features, BackendFeatures.SupportsResume, vtable.resume)
                && Has(features, BackendFeatures.SupportsSetVolume, vtable.set_volume)
                && Has(features, BackendFeatures.SupportsGetVolume, vtable.get_volume)
                && Has(features, BackendFeatures.SupportsSetRate, vtable.set_rate)
                && Has(features, BackendFeatures.SupportsGetRate, vtable.get_rate)
                && Has(features, BackendFeatures.SupportsSetPitch, vtable.set_pitch)
                && Has(features, BackendFeatures.SupportsGetPitch, vtable.get_pitch)
                && Has(features, BackendFeatures.SupportsRefreshVoices, vtable.refresh_voices)
                && Has(features, BackendFeatures.SupportsCountVoices, vtable.count_voices)
                && Has(features, BackendFeatures.SupportsGetVoiceName, vtable.get_voice_name)
                && Has(features, BackendFeatures.SupportsGetVoiceLanguage, vtable.get_voice_language)
                && Has(features, BackendFeatures.SupportsGetVoice, vtable.get_voice)
                && Has(features, BackendFeatures.SupportsSetVoice, vtable.set_voice)
                && Has(features, BackendFeatures.SupportsGetChannels, vtable.get_channels)
                && Has(features, BackendFeatures.SupportsGetSampleRate, vtable.get_sample_rate)
                && Has(features, BackendFeatures.SupportsGetBitDepth, vtable.get_bit_depth);
        }

        private static bool Has(ulong features, BackendFeatures feature, IntPtr member)
        {
            var present = member != IntPtr.Zero;
            var declared = (features & (ulong)feature) != 0;
            return present == declared;
        }

        private static IEnumerable<Delegate> CollectDelegates(BackendVTable vtable)
        {
            if (vtable.Create != null) yield return vtable.Create;
            if (vtable.Destroy != null) yield return vtable.Destroy;
            if (vtable.IsSupported != null) yield return vtable.IsSupported;
            if (vtable.Initialize != null) yield return vtable.Initialize;
            if (vtable.Speak != null) yield return vtable.Speak;
            if (vtable.SpeakToMemory != null) yield return vtable.SpeakToMemory;
            if (vtable.Braille != null) yield return vtable.Braille;
            if (vtable.Output != null) yield return vtable.Output;
            if (vtable.Stop != null) yield return vtable.Stop;
            if (vtable.Pause != null) yield return vtable.Pause;
            if (vtable.Resume != null) yield return vtable.Resume;
            if (vtable.IsSpeaking != null) yield return vtable.IsSpeaking;
            if (vtable.SetVolume != null) yield return vtable.SetVolume;
            if (vtable.GetVolume != null) yield return vtable.GetVolume;
            if (vtable.SetRate != null) yield return vtable.SetRate;
            if (vtable.GetRate != null) yield return vtable.GetRate;
            if (vtable.SetPitch != null) yield return vtable.SetPitch;
            if (vtable.GetPitch != null) yield return vtable.GetPitch;
            if (vtable.RefreshVoices != null) yield return vtable.RefreshVoices;
            if (vtable.CountVoices != null) yield return vtable.CountVoices;
            if (vtable.GetVoiceName != null) yield return vtable.GetVoiceName;
            if (vtable.GetVoiceLanguage != null) yield return vtable.GetVoiceLanguage;
            if (vtable.SetVoice != null) yield return vtable.SetVoice;
            if (vtable.GetVoice != null) yield return vtable.GetVoice;
            if (vtable.GetChannels != null) yield return vtable.GetChannels;
            if (vtable.GetSampleRate != null) yield return vtable.GetSampleRate;
            if (vtable.GetBitDepth != null) yield return vtable.GetBitDepth;
        }
    }
}
