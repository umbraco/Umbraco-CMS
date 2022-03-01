using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.DistributedLocking;

[TestFixture]
[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1133:Do not combine attributes", Justification = "YOLO")]
internal class InMemoryDistributedLockingMechanismTests
{
    internal class ReadLockTests
    {
        [Test, AutoMoqData]
        public void ReadLock_WithExistingReadLockForSameLockId_CanStillAcquire(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            var a = sut.ReadTester(aLockId, TimeSpan.FromSeconds(1)).Run();
            var b = sut.ReadTester(aLockId, TimeSpan.FromSeconds(1)).Run();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(a.Success);
                Assert.IsTrue(b.Success);
            });
        }

        [Test, AutoMoqData]
        public void ReadLock_WhenExistingWriteLockForSameLockId_SucceedsIfCanAcquireBeforeTimeout(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            var releaseHandle = sut.WriteLock(aLockId, TimeSpan.Zero);

            var tester = sut.ReadTester(aLockId, TimeSpan.FromSeconds(1));
            var thread = new Thread(tester.ThreadStart);
            thread.Start();

            Thread.Sleep(500);
            releaseHandle.Dispose();
            thread.Join();

            Assert.True(tester.Success);
        }

        [Test, AutoMoqData]
        public void ReadLock_WithExistingWriteLockForSameLockId_FailsToAcquireIfTimeoutExceeded(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            var a = sut.WriteTester(aLockId, TimeSpan.FromSeconds(1)).Run();
            var b = sut.ReadTester(aLockId, TimeSpan.FromSeconds(1)).Run();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(a.Success);
                Assert.IsFalse(b.Success);
            });
        }
    }

    internal class WriteLockTests
    {
        [Test, AutoMoqData]
        public void WriteLock_WithExistingReadLockForSameLockId_SucceedsIfCanAcquireBeforeTimeout(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            var releaseHandle = sut.ReadLock(aLockId, TimeSpan.Zero);

            var tester = sut.WriteTester(aLockId, TimeSpan.FromSeconds(1));
            var thread = new Thread(tester.ThreadStart);
            thread.Start();

            Thread.Sleep(500);
            releaseHandle.Dispose();
            thread.Join();

            Assert.True(tester.Success);
        }

        [Test, AutoMoqData]
        public void WriteLock_WithExistingWriteLockForSameLockId_SucceedsIfCanAcquireBeforeTimeout(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            var releaseHandle = sut.WriteLock(aLockId, TimeSpan.Zero);

            var tester = sut.WriteTester(aLockId, TimeSpan.FromSeconds(1));
            var thread = new Thread(tester.ThreadStart);
            thread.Start();

            Thread.Sleep(500);
            releaseHandle.Dispose();
            thread.Join();

            Assert.True(tester.Success);
        }

        [Test, AutoMoqData]
        public void WriteLock_WithExistingReadLockForSameLockId_FailsToAcquireIfTimeoutExceeded(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            var a = sut.ReadTester(aLockId, TimeSpan.FromSeconds(1)).Run();
            var b = sut.WriteTester(aLockId, TimeSpan.FromSeconds(1)).Run();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(a.Success);
                Assert.IsFalse(b.Success);
            });
        }

        [Test, AutoMoqData]
        public void WriteLock_WithExistingWriteLockForSameLockId_FailsToAcquireIfTimeoutExceeded(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            var a = sut.WriteTester(aLockId, TimeSpan.FromSeconds(1)).Run();
            var b = sut.WriteTester(aLockId, TimeSpan.FromSeconds(1)).Run();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(a.Success);
                Assert.IsFalse(b.Success);
            });
        }

        [Test, AutoMoqData]
        public void WriteLock_MultipleWriteLocks_PossibleForDifferentLockIds(InMemoryDistributedLockingMechanism sut)
        {
            var testers = new List<LockTester>();
            for (var i = 0; i < 10; i++)
            {
                testers.Add(sut.WriteTester(i, TimeSpan.FromSeconds(1)));
            }

            foreach (var lockTester in testers)
            {
                lockTester.Run();
            }

            Assert.True(testers.All(x => x.Success));
        }

        [Test, AutoMoqData]
        public void WriteLock_MultipleWriteLocks_OnlySingleWriterCanAcquire(
            InMemoryDistributedLockingMechanism sut,
            int aLockId)
        {
            const int numThreads = 4;

            var threads = new Thread[numThreads];
            var testers = new LockTester[numThreads];

            for (var i = 0; i < numThreads; i++)
            {
                testers[i] = sut.WriteTester(aLockId, TimeSpan.FromSeconds(1));
                threads[i] = new Thread(testers[i].ThreadStart);
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(1, testers.Count(x => x.Success));
        }
    }
}


[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Seems reasonable")]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "YOLO")]
internal static class DistributedLockingMechanismExtensions
{
    public static LockTester ReadTester(this IDistributedLockingMechanism lockingMechanism, int lockId, TimeSpan timeout)
        => new(lockingMechanism, lockId, false, timeout);

    public static LockTester WriteTester(this IDistributedLockingMechanism lockingMechanism, int lockId, TimeSpan timeout)
        => new(lockingMechanism, lockId, true, timeout);
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Seems reasonable")]
internal class LockTester : IDisposable
{
    private readonly IDistributedLockingMechanism _lockingMechanism;
    private readonly int _lockId;
    private readonly bool _write;
    private readonly TimeSpan _timeout;
    private IDistributedLock _lock;

    public bool Success => Exception == null;

    // ReSharper disable once MemberCanBePrivate.Global
    public DistributedLockingTimeoutException Exception { get; private set; }

    public LockTester(IDistributedLockingMechanism lockingMechanism, int lockId, bool write, TimeSpan timeout)
    {
        _lockingMechanism = lockingMechanism;
        _lockId = lockId;
        _write = write;
        _timeout = timeout;
    }

    public LockTester Run()
    {
        try
        {
            _lock = _write
                ? _lockingMechanism.WriteLock(_lockId, _timeout)
                : _lockingMechanism.ReadLock(_lockId, _timeout);
        }
        catch (DistributedLockingTimeoutException ex)
        {
            Exception = ex;
        }

        return this;
    }

    public void Dispose() => _lock?.Dispose();

    public void ThreadStart() => _ = Run();
}
