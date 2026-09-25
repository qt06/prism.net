# Prism.Net

.NET binding for [Prism](https://github.com/ethindp/prism) - the
Platform-agnostic Reader Interface for Speech and Messages. Prism unifies
screen-reader abstraction libraries (SpeechCore, UniversalSpeech, SRAL, Tolk,
...) and traditional TTS engines behind a single C API.

## Requirements

- .NET Framework 4.8+, .NET Standard 2.0+, or .NET 10+
- Native `prism` library (v0.18.2+) available in the application's working
  directory or `PATH` (for example `prism.dll` on Windows)

The native library is not part of the managed package. Obtain it from the
[Prism repository](https://github.com/ethindp/prism)
(or `vcpkg install ethindp-prism`) and deploy it alongside your application.

## Installation

Prism.Net is not published on NuGet.org. To use it, either reference
`src/Prism/Prism.csproj` directly, or build the package locally and add it from
a local feed:

```text
dotnet build src/Prism/Prism.csproj -c Release
dotnet nuget add source src/Prism/bin/Release --name prism-local
dotnet add package Prism.Net
```

## Supported targets

| Target framework | Notes |
| --- | --- |
| `net10.0` | Modern .NET; annotated for trimming and Native AOT |
| `net48` | .NET Framework 4.8 |
| `netstandard2.0` | .NET Core 2.0+, .NET 5-9, Mono/Unity, Xamarin |

Native AOT is supported; see the `Demo.Aot` sample.

## Usage

```csharp
using var ctx = new Prism.Context();
using var backend = ctx.CreateBest();
backend.Speak("Hello, World!");

// With audio callback
backend.SpeakToMemory("Hello", (samples, channels, rate) => {
    // Process audio samples (interleaved float array)
});

// Parameter control
backend.Volume = 0.8f;
backend.Rate = 0.6f;
backend.Pitch = 0.5f;

// Library version
Console.WriteLine(Prism.PrismInfo.VersionString);
```

### Custom backends

```csharp
using var builder = new Prism.Primitives.RegistryBuilder();
var id = builder.AddBackend("my-backend", 100, Prism.Primitives.BackendFeatures.SupportsSpeak,
    new Prism.Primitives.BackendVTable { Speak = (instance, text, interrupt) => { /* ... */ return 0; } });
var registry = builder.Freeze();
using var ctx = new Prism.Context(new Prism.Primitives.PrismConfig { Registry = registry });
```

### Logging

```csharp
Prism.Logger.SetHandler((level, source, message) => Console.WriteLine($"[{level}] {source}: {message}"));
```

> Note: dispose `Backend` objects before disposing their `Context`; the context
> disposes surviving backends automatically when disposed.

## Building from source

```text
dotnet build -c Release
dotnet test  -c Release      # net10.0 and, on Windows, net48
```

The prebuilt native library is expected at `prism_x64.dll` (or `prism_x86.dll`)
in the repository root; it is copied next to each test and sample binary on
build.

Samples under `demos/`:

| Project | Description |
| --- | --- |
| `Demo.Console` | Minimal walkthrough of the API |
| `Demo.WinForms` | Interactive WinForms demo |
| `Demo.Aot` | Native AOT sample (`dotnet publish -c Release -r win-x64`) |

## License

MPL-2.0. See [LICENSE](LICENSE) and [NOTICE](NOTICE).
