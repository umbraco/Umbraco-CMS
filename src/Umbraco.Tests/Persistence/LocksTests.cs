﻿using System;
using System.Data.SqlServerCe;
using System.Linq;
using System.Threading;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [Timeout(60000)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Serilog)]
    public class LocksTests : TestWithDatabaseBase
    {
        protected override void Initialize()
        {
            base.Initialize();

            // create a few lock objects
            using (var scope = ScopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 1, Name = "Lock.1" });
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 2, Name = "Lock.2" });
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 3, Name = "Lock.3" });
                scope.Complete();
            }
        }

        [Test]
        public void SingleReadLockTest()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void ConcurrentReadersTest()
        {
            const int threadCount = 8;
            var threads = new Thread[threadCount];
            var exceptions = new Exception[threadCount];
            var locker = new object();
            var acquired = 0;
            var m2 = new ManualResetEventSlim(false);
            var m1 = new ManualResetEventSlim(false);

            for (var i = 0; i < threadCount; i++)
            {
                var ic = i; // capture
                threads[i] = new Thread(() =>
                {
                    using (var scope = ScopeProvider.CreateScope())
                    {
                        try
                        {
                            scope.ReadLock(Constants.Locks.Servers);
                            lock (locker)
                            {
                                acquired++;
                                if (acquired == threadCount) m2.Set();
                            }
                            m1.Wait();
                            lock (locker)
                            {
                                acquired--;
                            }
                        }
                        catch (Exception e)
                        {
                            exceptions[ic] = e;
                        }
                        scope.Complete();
                    }
                });
            }

            // safe call context ensures that current scope does not leak into starting threads
            using (new SafeCallContext())
            {
                foreach (var thread in threads) thread.Start();
            }

            m2.Wait();
            // all threads have locked in parallel
            var maxAcquired = acquired;
            m1.Set();

            foreach (var thread in threads) thread.Join();

            Assert.AreEqual(threadCount, maxAcquired);
            Assert.AreEqual(0, acquired);

            for (var i = 0; i < threadCount; i++)
                Assert.IsNull(exceptions[i]);
        }

        [Test]
        public void ConcurrentWritersTest()
        {
            const int threadCount = 8;
            var threads = new Thread[threadCount];
            var exceptions = new Exception[threadCount];
            var locker = new object();
            var acquired = 0;
            var entered = 0;
            var ms = new AutoResetEvent[threadCount];
            for (var i = 0; i < threadCount; i++) ms[i] = new AutoResetEvent(false);
            var m1 = new ManualResetEventSlim(false);

            for (var i = 0; i < threadCount; i++)
            {
                var ic = i; // capture
                threads[i] = new Thread(() =>
                {
                    using (var scope = ScopeProvider.CreateScope())
                    {
                        try
                        {
                            lock (locker)
                            {
                                entered++;
                                if (entered == threadCount) m1.Set();
                            }
                            ms[ic].WaitOne();
                            scope.WriteLock(Constants.Locks.Servers);
                            lock (locker)
                            {
                                acquired++;
                            }
                            ms[ic].WaitOne();
                            lock (locker)
                            {
                                acquired--;
                            }
                        }
                        catch (Exception e)
                        {
                            exceptions[ic] = e;
                        }
                        scope.Complete();
                    }
                });
            }

            // safe call context ensures that current scope does not leak into starting threads
            using (new SafeCallContext())
            {
                foreach (var thread in threads) thread.Start();
            }

            m1.Wait();
            // all threads have entered
            ms[0].Set(); // let 0 go
            Thread.Sleep(100);
            for (var i = 1; i < threadCount; i++) ms[i].Set(); // let others go
            Thread.Sleep(500);
            // only 1 thread has locked
            Assert.AreEqual(1, acquired);
            for (var i = 0; i < threadCount; i++) ms[i].Set(); // let all go

            foreach (var thread in threads) thread.Join();

            Assert.AreEqual(0, acquired);

            for (var i = 0; i < threadCount; i++)
                Assert.IsNull(exceptions[i]);
        }

        [Test]
        public void DeadLockTest()
        {
            Exception e1 = null, e2 = null;
            AutoResetEvent ev1 = new AutoResetEvent(false), ev2 = new AutoResetEvent(false);

            // testing:
            // two threads will each obtain exclusive write locks over two
            // identical lock objects deadlock each other

            var thread1 = new Thread(() => DeadLockTestThread(1, 2, ev1, ev2, ref e1));
            var thread2 = new Thread(() => DeadLockTestThread(2, 1, ev2, ev1, ref e2));

            // safe call context ensures that current scope does not leak into starting threads
            using (new SafeCallContext())
            {
                thread1.Start();
                thread2.Start();
            }

            ev2.Set();

            thread1.Join();
            thread2.Join();

            Assert.IsNotNull(e1);
            Assert.IsInstanceOf<SqlCeLockTimeoutException>(e1);

            // the assertion below depends on timing conditions - on a fast enough environment,
            // thread1 dies (deadlock) and frees thread2, which succeeds - however on a slow
            // environment (CI) both threads can end up dying due to deadlock - so, cannot test
            // that e2 is null - but if it's not, can test that it's a timeout
            //
            //Assert.IsNull(e2);
            if (e2 != null)
                Assert.IsInstanceOf<SqlCeLockTimeoutException>(e2);
        }

        private void DeadLockTestThread(int id1, int id2, EventWaitHandle myEv, WaitHandle otherEv, ref Exception exception)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    otherEv.WaitOne();
                    Console.WriteLine($"[{id1}] WAIT {id1}");
                    scope.WriteLock(id1);
                    Console.WriteLine($"[{id1}] GRANT {id1}");
                    WriteLocks(scope.Database);
                    myEv.Set();

                    if (id1 == 1)
                        otherEv.WaitOne();
                    else
                        Thread.Sleep(200); // cannot wait due to deadlock... just give it a bit of time

                    Console.WriteLine($"[{id1}] WAIT {id2}");
                    scope.WriteLock(id2);
                    Console.WriteLine($"[{id1}] GRANT {id2}");
                    WriteLocks(scope.Database);
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    scope.Complete();
                }
            }
        }

        [Test]
        public void NoDeadLockTest()
        {
            Exception e1 = null, e2 = null;
            AutoResetEvent ev1 = new AutoResetEvent(false), ev2 = new AutoResetEvent(false);

            // testing:
            // two threads will each obtain exclusive write lock over two
            // different lock objects without blocking each other

            var thread1 = new Thread(() => NoDeadLockTestThread(1, ev1, ev2, ref e1));
            var thread2 = new Thread(() => NoDeadLockTestThread(2, ev2, ev1, ref e1));

            // need safe call context else the current one leaks into *both* threads
            using (new SafeCallContext())
            {
                thread1.Start();
                thread2.Start();
            }

            ev2.Set();

            thread1.Join();
            thread2.Join();

            Assert.IsNull(e1);
            Assert.IsNull(e2);
        }

        private void NoDeadLockTestThread(int id, EventWaitHandle myEv, WaitHandle otherEv, ref Exception exception)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                try
                {
                    otherEv.WaitOne();
                    Console.WriteLine($"[{id}] WAIT {id}");
                    scope.WriteLock(id);
                    Console.WriteLine($"[{id}] GRANT {id}");
                    WriteLocks(scope.Database);
                    myEv.Set();
                    otherEv.WaitOne();
                }
                catch (Exception e)
                {
                    exception = e;
                }
                finally
                {
                    scope.Complete();
                    myEv.Set();
                }
            }
        }

        private void WriteLocks(IDatabaseQuery database)
        {
            Console.WriteLine("LOCKS:");
            var info = database.Query<dynamic>("SELECT * FROM sys.lock_information;").ToList();
            foreach (var row in info)
                Console.WriteLine(string.Format("> {0} {1} {2} {3} {4} {5} {6}", row.request_spid,
                    row.resource_type, row.resource_description, row.request_mode, row.resource_table,
                    row.resource_table_id, row.request_status));
        }
    }
}
