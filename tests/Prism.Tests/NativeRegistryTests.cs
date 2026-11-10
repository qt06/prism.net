// SPDX-License-Identifier: MPL-2.0

using System;
using System.Runtime.InteropServices;
using Prism.Exceptions;
using Prism.Primitives;
using Xunit;

namespace Prism.Tests
{
    public class NativeRegistryTests
    {
        [NativeFact]
        public void Registry_Count_IsPositive()
        {
            using var context = new Context();
            Assert.True(context.BackendsCount > 0, "registry has no backends");
        }

        [NativeFact]
        public void Registry_Enumerates_NamesAndPriorities()
        {
            using var context = new Context();
            for (var i = 0; i < context.BackendsCount; i++)
            {
                var id = context.IdAt(i);
                Assert.NotEqual(BackendId.INVALID, id);
                Assert.False(string.IsNullOrEmpty(context.NameOf(id)));
                Assert.True(context.PriorityOf(id) >= 0);
            }
        }

        [NativeFact]
        public void Get_ReturnsCachedBackend()
        {
            using var context = new Context();
            if (!context.Exists(BackendId.SAPI))
            {
                return;
            }

            using var acquired = context.Acquire(BackendId.SAPI);
            using var cached = context.Get(BackendId.SAPI);
            Assert.False(string.IsNullOrEmpty(cached.Name));
        }

        [NativeFact]
        public void RegistryBuilder_AddBackend_EmptyName_Throws()
        {
            using var builder = new RegistryBuilder();
            Assert.Throws<ArgumentException>(
                () => builder.AddBackend("", 0, BackendFeatures.SupportsSpeak, new BackendVTable()));
        }

        [NativeFact]
        public void RegistryBuilder_AddBackend_NullVTable_Throws()
        {
            using var builder = new RegistryBuilder();
            Assert.Throws<ArgumentNullException>(
                () => builder.AddBackend("test", 0, BackendFeatures.SupportsSpeak, null!));
        }

        [NativeFact]
        public void CustomBackend_RegistersAndSpeaks()
        {
            var spoke = false;
            var userdata = GCHandle.Alloc(new object(), GCHandleType.Normal);
            try
            {
                using var builder = new RegistryBuilder();
                var vtable = new BackendVTable
                {
                    Create = _ => GCHandle.ToIntPtr(GCHandle.Alloc(new EchoState(), GCHandleType.Normal)),
                    Destroy = instance => ((GCHandle)instance).Free(),
                    IsSupported = _ => true,
                    Initialize = _ => 0,
                    Speak = (instance, text, interrupt) =>
                    {
                        spoke = true;
                        return 0;
                    },
                    Stop = _ => 0,
                    IsSpeaking = (IntPtr instance, out bool speaking) => { speaking = false; return 0; },
                    SetVoice = (IntPtr instance, UIntPtr voiceId) => 0,
                    GetVoice = (IntPtr instance, out UIntPtr voiceId) => { voiceId = UIntPtr.Zero; return 0; }
                };
                var customId = builder.AddBackend("dotnet-echo", 50,
                    BackendFeatures.SupportsSpeak | BackendFeatures.SupportsStop |
                    BackendFeatures.SupportsIsSpeaking |
                    BackendFeatures.SupportsSetVoice | BackendFeatures.SupportsGetVoice,
                    vtable, GCHandle.ToIntPtr(userdata));
                using var registry = builder.Freeze();

                using var context = new Context(new PrismConfig { Registry = registry });
                Assert.True(context.Exists(customId));

                using var echo = context.Acquire(customId);
                echo.Speak("Hello from a C# custom backend");
                Assert.True(spoke);
            }
            finally
            {
                userdata.Free();
            }
        }

        private sealed class EchoState
        {
        }

        [NativeFact]
        public void CustomBackend_RequiresExplicitSelection_IsSkippedByBest()
        {
            var created = 0;
            var spoke = false;
            var userdata = GCHandle.Alloc(new object(), GCHandleType.Normal);
            try
            {
                using var builder = new RegistryBuilder();
                var vtable = new BackendVTable
                {
                    Create = _ =>
                    {
                        created++;
                        return GCHandle.ToIntPtr(GCHandle.Alloc(new EchoState(), GCHandleType.Normal));
                    },
                    Destroy = instance => ((GCHandle)instance).Free(),
                    IsSupported = _ => true,
                    Initialize = _ => 0,
                    Speak = (instance, text, interrupt) =>
                    {
                        spoke = true;
                        return 0;
                    },
                    Stop = _ => 0,
                    IsSpeaking = (IntPtr instance, out bool speaking) => { speaking = false; return 0; }
                };
                var customId = builder.AddBackend("dotnet-explicit", 1000,
                    BackendFeatures.SupportsSpeak | BackendFeatures.SupportsStop |
                    BackendFeatures.SupportsIsSpeaking |
                    BackendFeatures.RequiresExplicitSelection,
                    vtable, GCHandle.ToIntPtr(userdata));
                using var registry = builder.Freeze();

                using var context = new Context(new PrismConfig { Registry = registry });
                Assert.True(context.Exists(customId));

                // Best selection must skip the explicit backend. Another
                // compiled-in backend may or may not be available.
                try
                {
                    using var best = context.CreateBest();
                }
                catch (PrismBackendNotAvailableException)
                {
                }
                Assert.Equal(0, created);

                using var explicitBackend = context.Create(customId);
                explicitBackend.Speak("explicit selection");
                Assert.Equal(1, created);
                Assert.True(spoke);
            }
            finally
            {
                userdata.Free();
            }
        }

        [NativeFact]
        public void CustomBackend_CallbackException_BecomesPrismError()
        {
            var userdata = GCHandle.Alloc(new object(), GCHandleType.Normal);
            try
            {
                using var builder = new RegistryBuilder();
                var vtable = new BackendVTable
                {
                    Create = _ => GCHandle.ToIntPtr(GCHandle.Alloc(new EchoState(), GCHandleType.Normal)),
                    Destroy = instance => ((GCHandle)instance).Free(),
                    IsSupported = _ => true,
                    Initialize = _ => 0,
                    Speak = (instance, text, interrupt) => throw new InvalidOperationException("boom"),
                    Stop = _ => 0
                };
                var customId = builder.AddBackend("dotnet-throwing", 1000,
                    BackendFeatures.SupportsSpeak | BackendFeatures.SupportsStop,
                    vtable, GCHandle.ToIntPtr(userdata));
                using var registry = builder.Freeze();
                using var context = new Context(new PrismConfig { Registry = registry });
                using var backend = context.Create(customId);

                // The managed exception must be contained at the reverse
                // P/Invoke boundary and surface as a Prism error instead of
                // unwinding through native code.
                Assert.Throws<PrismInternalException>(() => backend.Speak("boom"));
            }
            finally
            {
                userdata.Free();
            }
        }

        [NativeFact]
        public void RegistryBuilder_FreezeTwice_ThrowsInvalidOperation()
        {
            using var builder = new RegistryBuilder();
            using var registry = builder.Freeze();
            Assert.Throws<PrismInvalidOperationException>(() => builder.Freeze());
        }
    }
}
