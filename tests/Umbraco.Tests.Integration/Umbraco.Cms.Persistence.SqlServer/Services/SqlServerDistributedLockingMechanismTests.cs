using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lucene.Net.Analysis.Cjk;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Persistence.SqlServer.Services.Tests;

// TODO: Should be in new test project.
[TestFixture]
public class SqlServerDistributedLockingMechanismTests
{
    public abstract class SqlServerDistributedLockingTestBase : UmbracoIntegrationTest
    {
        protected List<LockTester> Testers { get; private set; }

        protected SqlServerDistributedLockingMechanism Sut =>
            new(
                GetRequiredService<ILogger<SqlServerDistributedLockingMechanism>>(),
                GetRequiredService<IUmbracoDatabaseFactory>(),
                GetRequiredService<IOptionsMonitor<GlobalSettings>>());

        [SetUp]
        public void TestSetup() => Testers = new List<LockTester>();

        [TearDown]
        public void TestTearDown()
        {
            foreach (var tester in Testers)
            {
                tester.Dispose();
            }
        }
    }

    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ReadLockTests : SqlServerDistributedLockingTestBase
    {
        [Test]
        public void ReadLock_WithExistingReadLockForSameLockId_CanStillAcquire()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            Testers.Add(Sut.ReadTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
            Testers.Add(Sut.ReadTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));

            foreach (var tester in Testers)
            {
                tester.Run();
                Thread.Sleep(100);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(Testers.First().Success);
                Assert.IsTrue(Testers.Last().Success);
            });
        }

        [Test]
        public void ReadLock_WhenExistingWriteLockForSameLockId_SucceedsIfCanAcquireBeforeTimeout()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            var releaseHandle = Sut.WriteLock(Cms.Core.Constants.Locks.Domains, TimeSpan.Zero);

            var tester = Sut.ReadTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1));
            var thread = new Thread(tester.ThreadStart);
            thread.Start();

            Thread.Sleep(500);
            releaseHandle.Dispose();
            thread.Join();

            Assert.True(tester.Success);
        }

        [Test]
        public void ReadLock_WithExistingWriteLockForSameLockId_FailsToAcquireIfTimeoutExceeded()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            Testers.Add(Sut.ReadTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));

            foreach (var tester in Testers)
            {
                tester.Run();
                Thread.Sleep(100);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(Testers.First().Success);
                Assert.IsFalse(Testers.Last().Success);
            });
        }
    }

    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class WriteLockTests : SqlServerDistributedLockingTestBase
    {
        [Test]
        public void WriteLock_WithExistingReadLockForSameLockId_SucceedsIfCanAcquireBeforeTimeout()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            var releaseHandle = Sut.ReadLock(Cms.Core.Constants.Locks.Domains, TimeSpan.Zero);

            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
            var tester = Testers.Single();

            var thread = new Thread(tester.ThreadStart);
            thread.Start();

            Thread.Sleep(500);
            releaseHandle.Dispose();
            thread.Join();

            Assert.True(tester.Success);
        }

        [Test]
        public void WriteLock_WithExistingWriteLockForSameLockId_SucceedsIfCanAcquireBeforeTimeout()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            var releaseHandle = Sut.WriteLock(Cms.Core.Constants.Locks.Domains, TimeSpan.Zero);

            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
            var tester = Testers.Single();

            var thread = new Thread(tester.ThreadStart);
            thread.Start();

            Thread.Sleep(500);
            releaseHandle.Dispose();
            thread.Join();

            Assert.True(tester.Success);
        }

        [Test]
        public void WriteLock_WithExistingReadLockForSameLockId_FailsToAcquireIfTimeoutExceeded()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            Testers.Add(Sut.ReadTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));

            foreach (var tester in Testers)
            {
                tester.Run();
                Thread.Sleep(100);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(Testers.First().Success);
                Assert.IsFalse(Testers.Last().Success);
            });
        }

        [Test]
        public void WriteLock_WithExistingWriteLockForSameLockId_FailsToAcquireIfTimeoutExceeded()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));

            foreach (var tester in Testers)
            {
                tester.Run();
                Thread.Sleep(100);
            }

            Assert.Multiple(() =>
            {
                Assert.IsTrue(Testers.First().Success);
                Assert.IsFalse(Testers.Last().Success);
            });
        }

        [Test]
        public void WriteLock_MultipleWriteLocks_PossibleForDifferentLockIds()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Servers, TimeSpan.FromSeconds(1)));
            Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Languages, TimeSpan.FromSeconds(1)));

            foreach (var tester in Testers)
            {
                tester.Run();
                Thread.Sleep(100);
            }

            Assert.True(Testers.All(x => x.Success));
        }

        [Test]
        public void WriteLock_MultipleWriteLocks_OnlySingleWriterCanAcquire()
        {
            if (!BaseTestDatabase.IsSqlServer())
            {
                Assert.Ignore("Requires SQL Server to run");
            }

            const int numThreads = 4;

            var threads = new Thread[numThreads];

            for (var i = 0; i < numThreads; i++)
            {
                Testers.Add(Sut.WriteTester(Cms.Core.Constants.Locks.Domains, TimeSpan.FromSeconds(1)));
                threads[i] = new Thread(Testers[i].ThreadStart);
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(1, Testers.Count(x => x.Success));
        }
    }
}
