# Prism.Net Agent Guide

## Project

Prism.Net is a C# P/Invoke binding for the native Prism library - the
Platform-agnostic Reader Interface for Speech and Messages.

- Upstream native project: https://github.com/ethindp/prism
- Library targets: `net10.0`, `net48`, and `netstandard2.0`
- Tests target: `net10.0` (plus `net48` on Windows)
- Main source: `src/Prism/`
- Tests: `tests/Prism.Tests/`
- Demos: `demos/` (`Demo.Console`, `Demo.WinForms`, Native AOT `Demo.Aot`)
- Package: `Prism.Net` (id/version in `src/Prism/Prism.csproj` and `build/Prism.props`)
- License: MPL-2.0

The native API source of truth is the sibling `../prism` project, especially
`../prism/include/prism.h` and `../prism/doc/src/api/`.

## Native Dependency

The managed library requires the native `prism` library at runtime. The native
library is not automatically supplied by the managed project and must be
available through the application's working directory or `PATH`.

For this repository the prebuilt native binaries live at the repository root as
`prism_x64.dll` / `prism_x86.dll`; tests and demos copy the matching file to
their output directory as `prism.dll`.

When changing P/Invoke declarations, callbacks, structs, ownership, or
delegate lifetimes, verify the ABI and lifetime rules against the native header
and documentation before changing managed code.

## Native AOT

The `net10.0` target sets `IsAotCompatible`, so builds must stay free of
`IL2xxx`/`IL3xxx` trim/AOT warnings. `Demo.Aot` publishes a Native AOT binary
and is the end-to-end check for P/Invoke, delegate parameters, and reverse
P/Invoke (custom backend vtables).

Do not introduce reflection, `Assembly.Load*`, `Reflection.Emit`, or COM; none
are used today and they are unsupported under Native AOT.

## Commands

Run from the project directory:

```text
dotnet build --no-restore -c Release
dotnet test  --no-restore -c Release
```

Some tests require the native Prism library and are skipped when it is not
available. Do not treat skipped native integration tests as full coverage.

## Implementation Rules

- Keep P/Invoke signatures, calling conventions, native bool widths, and
  `size_t` mappings compatible with `prism.h`.
- Preserve native ownership and lifetime semantics for contexts, registries,
  backends, callbacks, and custom backend vtables.
- Keep managed delegates rooted for exactly as long as native code may invoke
  them.
- Prefer the smallest change that fixes the verified behavior.
- Add or update tests for managed behavior and native integration behavior when
  the native library is available.
- Do not modify generated build output under `bin/` or `obj/`.

## Packaging

The project generates the `Prism.Net` NuGet package on build. The package ships
`lib/net10.0`, `lib/net48`, and `lib/netstandard2.0`. Confirm native asset and
documentation behavior separately when changing packaging files.
