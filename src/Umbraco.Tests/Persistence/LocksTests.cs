using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.Services;
using Umbraco.Tests.TestHelpers;
using Ignore = NUnit.Framework.IgnoreAttribute;

namespace Umbraco.Tests.Persistence
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    [Ignore("Takes too much time.")]
    public class LocksTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            // create a few lock objects
            using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
            {
                var database = scope.Database;
                database.Execute("SET IDENTITY_INSERT umbracoLock ON");
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 1, Name = "Lock.1" });
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 2, Name = "Lock.2" });
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 3, Name = "Lock.3" });
                database.Execute("SET IDENTITY_INSERT umbracoLock OFF");
                scope.Complete();
            }
        }

        [Test]
        public void Test()
        {
            using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
            {
                var database = scope.Database;
                database.AcquireLockNodeReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void ConcurrentReadersTest()
        {
            var threads = new List<Thread>();
            var locker = new object();
            var acquired = 0;
            var m2 = new ManualResetEventSlim(false);
            var m1 = new ManualResetEventSlim(false);
            for (var i = 0; i < 5; i++)
            {
                threads.Add(new Thread(() =>
                {
                    // each thread gets its own scope, because it has its own LCC, hence its own database
                    using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
                    {
                        var database = scope.Database;
                        database.AcquireLockNodeReadLock(Constants.Locks.Servers);
                        lock (locker)
                        {
                            acquired++;
                            if (acquired == 5) m2.Set();
                        }
                        m1.Wait();
                        lock (locker)
                        {
                            acquired--;
                        }
                        scope.Complete();
                    }
                }));
            }
            foreach (var thread in threads) thread.Start();
            m2.Wait();
            // all threads have locked in parallel
            var maxAcquired = acquired;
            m1.Set();
            foreach (var thread in threads) thread.Join();
            Assert.AreEqual(5, maxAcquired);
            Assert.AreEqual(0, acquired);
        }

        [Test]
        public void ConcurrentWritersTest()
        {
            var threads = new List<Thread>();
            var locker = new object();
            var entered = 0;
            var ms = new AutoResetEvent[5];
            for (var i = 0; i < 5; i++) ms[i] = new AutoResetEvent(false);
            var m1 = new ManualResetEventSlim(false);
            var acquired = 0;
            for (var i = 0; i < 5; i++)
            {
                var ic = i;
                threads.Add(new Thread(() =>
                {
                    // each thread gets its own scope, because it has its own LCC, hence its own database
                    using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
                    {
                        var database = scope.Database;
                        lock (locker)
                        {
                            entered++;
                            if (entered == 5) m1.Set();
                        }
                        ms[ic].WaitOne();
                        database.AcquireLockNodeWriteLock(Constants.Locks.Servers);
                        lock (locker)
                        {
                            acquired++;
                        }
                        ms[ic].WaitOne();
                        lock (locker)
                        {
                            acquired--;
                        }
                        scope.Complete();
                    }
                }));
            }
            foreach (var thread in threads) thread.Start();
            m1.Wait();
            // all threads have entered
            ms[0].Set(); // let 0 go
            Thread.Sleep(100);
            for (var i = 1; i < 5; i++) ms[i].Set(); // let others go
            Thread.Sleep(500);
            // only 1 thread has locked
            Assert.AreEqual(1, acquired);
            for (var i = 0; i < 5; i++) ms[i].Set(); // let all go
            foreach (var thread in threads) thread.Join();
        }

        [Test]
        public void DeadLockTest()
        {
            Exception e1 = null, e2 = null;

            var thread1 = new Thread(() =>
            {
                // each thread gets its own scope, because it has its own LCC, hence its own database
                using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
                {
                    var database = scope.Database;
                    Console.WriteLine("Thread 1 db " + database.InstanceSid);
                    try
                    {
                        database.AcquireLockNodeWriteLock(1);
                        Thread.Sleep(100);
                        database.AcquireLockNodeWriteLock(2);
                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        e1 = e;
                    }
                    scope.Complete();
                }
            });
            var thread2 = new Thread(() =>
            {
                // each thread gets its own scope, because it has its own LCC, hence its own database
                using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
                {
                    var database = scope.Database;
                    Console.WriteLine("Thread 2 db " + database.InstanceSid);
                    try
                    {
                        database.AcquireLockNodeWriteLock(2);
                        Thread.Sleep(100);
                        database.AcquireLockNodeWriteLock(1);
                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        e2 = e;
                    }
                    scope.Complete();
                }
            });
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();
            var oneIsNotNull = e1 != null || e2 != null;
            Assert.IsTrue(oneIsNotNull);
            if (e1 != null)
                Assert.IsInstanceOf<SqlCeLockTimeoutException>(e1);
            if (e2 != null)
                Assert.IsInstanceOf<SqlCeLockTimeoutException>(e2);
        }

        [Test]
        public void NoDeadLockTest()
        {
            Exception e1 = null, e2 = null;

            var thread1 = new Thread(() =>
            {
                // each thread gets its own scope, because it has its own LCC, hence its own database
                using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
                {
                    var database = scope.Database;
                    try
                    {
                        database.AcquireLockNodeWriteLock(1);
                        var info = database.Query<dynamic>("SELECT * FROM sys.lock_information;");
                        Console.WriteLine("LOCKS:");
                        foreach (var row in info)
                            Console.WriteLine(string.Format("> {0} {1} {2} {3} {4} {5} {6}", row.request_spid,
                                row.resource_type, row.resource_description, row.request_mode, row.resource_table,
                                row.resource_table_id, row.request_status));
                        Thread.Sleep(6000);
                    }
                    catch (Exception e)
                    {
                        e1 = e;
                    }
                    scope.Complete();
                }
            });
            var thread2 = new Thread(() =>
            {
                // each thread gets its own scope, because it has its own LCC, hence its own database
                using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
                {
                    var database = scope.Database;
                    try
                    {
                        Thread.Sleep(1000);
                        database.AcquireLockNodeWriteLock(2);
                        var info = database.Query<dynamic>("SELECT * FROM sys.lock_information;");
                        Console.WriteLine("LOCKS:");
                        foreach (var row in info)
                            Console.WriteLine(string.Format("> {0} {1} {2} {3} {4} {5} {6}", row.request_spid,
                                row.resource_type, row.resource_description, row.request_mode, row.resource_table,
                                row.resource_table_id, row.request_status));
                        Thread.Sleep(1000);
                    }
                    catch (Exception e)
                    {
                        e2 = e;
                    }
                    scope.Complete();
                }
            });
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();
            Assert.IsNull(e1);
            Assert.IsNull(e2);
        }
    }
}
