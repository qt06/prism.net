// SPDX-License-Identifier: MPL-2.0

using System;
using Prism.Exceptions;
using Prism.Primitives;
using Xunit;

namespace Prism.Tests
{
    public class NativeContextTests
    {
        [NativeFact]
        public void Context_Constructor_CreatesValidContext()
        {
            using var context = new Context();
            Assert.NotNull(context);
        }

        [NativeFact]
        public void Context_BackendsCount_ReturnsNonNegative()
        {
            using var context = new Context();
            Assert.True(context.BackendsCount >= 0);
        }

        [NativeFact]
        public void Context_IdAt_ValidIndex_ReturnsValidId()
        {
            using var context = new Context();
            if (context.BackendsCount > 0)
            {
                var id = context.IdAt(0);
                Assert.NotEqual(BackendId.INVALID, id);
            }
        }

        [NativeFact]
        public void Context_Exists_WithInvalidId_ReturnsFalse()
        {
            using var context = new Context();
            Assert.False(context.Exists(BackendId.INVALID));
        }

        [NativeFact]
        public void Context_IdOf_EmptyName_ThrowsArgument()
        {
            using var context = new Context();
            Assert.Throws<ArgumentException>(() => context.IdOf(null!));
            Assert.Throws<ArgumentException>(() => context.IdOf(""));
        }

        [NativeFact]
        public void Context_Dispose_CanBeCalledMultipleTimes()
        {
            var context = new Context();
            context.Dispose();
            context.Dispose();
        }

        [NativeFact]
        public void Context_Dispose_DisposesTrackedBackends()
        {
            var context = new Context();
            using var backend = context.CreateBest();
            context.Dispose();
            Assert.Throws<PrismInvalidOperationException>(() => backend.Name);
        }

        [NativeFact]
        public void Context_Acquire_PreferredOrBest()
        {
            using var context = new Context();
            using var backend = context.Exists(BackendId.SAPI)
                ? context.Acquire(BackendId.SAPI)
                : context.AcquireBest();
            Assert.False(string.IsNullOrEmpty(backend.Name));
        }
    }
}
