// SPDX-License-Identifier: MPL-2.0

using Xunit;

// The native library keeps process-global state (log handler/level and the
// availability polling thread), so native integration tests must not run in
// parallel with each other.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
