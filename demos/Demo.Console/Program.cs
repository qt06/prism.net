using System;
using System.Runtime.InteropServices;
using Prism;
using Prism.Exceptions;
using Prism.Native;
using Prism.Primitives;

try
{
    Console.WriteLine("=== Prism Console Demo ===");
    Console.WriteLine($"CLR: {Environment.Version}");
    Console.WriteLine($"Process: {(Environment.Is64BitProcess ? "x64" : "x86")}");
    Console.WriteLine($"Native prism version: {PrismInfo.VersionString} (0x{PrismInfo.Version:X})");
    Console.WriteLine($"Plugin ABI: {PrismInfo.PluginAbiVersion}");
    Console.WriteLine();

    // --- Logger handler ---
    Logger.SetHandler((level, source, message) =>
        Console.WriteLine($"[log][{level}] {source}: {message}"));
    Logger.SetLevel(PrismLogLevel.Trace);
    Logger.Log(PrismLogLevel.Info, "console-demo", "log handler works");
    Logger.Flush();

    // --- Registry enumerations + backend creation ---
    using var ctx = new Context();
    Console.WriteLine($"Backends in registry: {ctx.BackendsCount}");
    for (var i = 0; i < ctx.BackendsCount; i++)
    {
        var id = ctx.IdAt(i);
        Console.WriteLine($"  [{i}] {ctx.NameOf(id)} (priority {ctx.PriorityOf(id)})");
    }

    using (var backend = ctx.CreateBest())
    {
        Console.WriteLine($"Best backend: {backend.Name}, speaking test...");
        backend.Speak("Prism .NET binding works");
        backend.Stop();

        // --- SpeakToMemory (fixed sample-count semantics) ---
        try
        {
            backend.SpeakToMemory("hello", (samples, channels, rate) =>
            {
                Console.WriteLine($"[SpeakToMemory] {samples.Length} floats, {channels} ch, {rate} Hz");
            });
        }
        catch (PrismException ex)
        {
            Console.WriteLine($"[SpeakToMemory] not supported here: {ex.GetType().Name}");
        }

        // --- Cached backend via registry_get (only acquire caches instances) ---
        if (ctx.Exists(BackendId.SAPI))
        {
            using var acquired = ctx.Acquire(BackendId.SAPI);
            using var cached = ctx.Get(BackendId.SAPI);
            Console.WriteLine($"registry_get: acquired='{acquired.Name}', cached='{cached.Name}' (shared instance: {ReferenceEquals(acquired, cached) == false}, same name: {acquired.Name == cached.Name})");
        }
    }

    // --- Custom backend via RegistryBuilder ---
    var userdata = GCHandle.Alloc(new EchoState(), GCHandleType.Normal);
    try
    {
        using var builder = new RegistryBuilder();
        var vtable = new BackendVTable
        {
            Create = u => GCHandle.ToIntPtr(GCHandle.Alloc(new EchoBackend(), GCHandleType.Normal)),
            Destroy = instance => ((GCHandle)instance).Free(),
            IsSupported = _ => true,
            Initialize = instance => { ((EchoBackend)((GCHandle)instance).Target!).Initialized = true; return 0; },
            Speak = (instance, text, interrupt) =>
            {
                Console.WriteLine($"[EchoBackend] speak: {PrismNativeLib.Utf8PtrToString(text)}");
                return 0;
            },
            Stop = _ => 0,
            IsSpeaking = (IntPtr instance, out bool speaking) => { speaking = false; return 0; },
            SetVoice = (IntPtr instance, UIntPtr voiceId) => 0,
            GetVoice = (IntPtr instance, out UIntPtr voiceId) => { voiceId = 0; return 0; }
        };
        var customId = builder.AddBackend("dotnet-echo", 50,
            BackendFeatures.SupportsSpeak | BackendFeatures.SupportsStop |
            BackendFeatures.SupportsIsSpeaking |
            BackendFeatures.SupportsSetVoice | BackendFeatures.SupportsGetVoice,
            vtable, GCHandle.ToIntPtr(userdata));
        using var registry = builder.Freeze();
        Console.WriteLine($"Custom backend registered: 0x{customId:X}");

        using var customCtx = new Context(new PrismConfig { Registry = registry });
        Console.WriteLine($"Custom registry count: {customCtx.BackendsCount}");
        var echoName = customCtx.NameOf(customId);
        Console.WriteLine($"Custom backend name: {echoName}");
        using var echo = customCtx.Acquire(customId);
        echo.Speak("Hello from a C# custom backend");
    }
    finally
    {
        userdata.Free();
    }

    // --- Availability callback ---
    var availabilitySeen = 0;
    var baselineSeen = 0;
    var cfg = PrismConfig.Create();
    cfg.AvailabilityCallback = (id, name, available) =>
    {
        availabilitySeen++;
        Console.WriteLine($"[availability] {name} available={available}");
    };
    cfg.AvailabilityBaselineCallback = () =>
    {
        baselineSeen++;
        Console.WriteLine("[availability] baseline established");
    };
    cfg.AvailabilityPollIntervalMs = 100;
    using (var pollCtx = new Context(cfg))
    {
        System.Threading.Thread.Sleep(400);
    }
    Console.WriteLine($"Availability events received: {availabilitySeen}, baselines: {baselineSeen}");

    Console.WriteLine("=== All done ===");
}
catch (Exception ex)
{
    Console.WriteLine($"\nFATAL: [{ex.GetType().Name}] {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

internal sealed class EchoState
{
    public string Tag => "echo-state";
}

internal sealed class EchoBackend
{
    public bool Initialized { get; set; }
}
