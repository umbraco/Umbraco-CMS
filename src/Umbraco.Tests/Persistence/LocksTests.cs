using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Threading;
using System.Web;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.DI;
using Umbraco.Web;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [Timeout(60000)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Log4Net)]
    public class LocksTests : TestWithDatabaseBase
    {
        protected override void Compose()
        {
            base.Compose();

            // cannot use the default TestUmbracoDatabaseAccessor which only handles one instance at a time

            // works with...
            //Container.RegisterSingleton<IDatabaseFactory, ThreadSafetyServiceTest.PerThreadSqlCeDatabaseFactory>();

            // but it should work with...
            Container.RegisterSingleton<IUmbracoDatabaseAccessor, HybridUmbracoDatabaseAccessor>();
            Container.RegisterSingleton<IHttpContextAccessor, NoHttpContextAccessor>();
            // + using SafeCallContext when starting threads

            // fixme - need to ensure that disposing of the DB properly removes it from context see 7.6
        }

        private class NoHttpContextAccessor : IHttpContextAccessor
        {
            public HttpContext HttpContext { get; set; } = null;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // create a few lock objects
            var database = DatabaseContext.Database;
            database.BeginTransaction(IsolationLevel.RepeatableRead);
            try
            {
                database.Execute("SET IDENTITY_INSERT umbracoLock ON");
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 1, Name = "Lock.1" });
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 2, Name = "Lock.2" });
                database.Insert("umbracoLock", "id", false, new LockDto { Id = 3, Name = "Lock.3" });
                database.Execute("SET IDENTITY_INSERT umbracoLock OFF");
                database.CompleteTransaction();
            }
            catch
            {
                database.AbortTransaction();
            }
        }

        [Test]
        public void SingleReadLockTest()
        {
            var database = DatabaseContext.Database;
            database.BeginTransaction(IsolationLevel.RepeatableRead);
            try
            {
                database.AcquireLockNodeReadLock(Constants.Locks.Servers);
            }
            finally
            {
                database.CompleteTransaction();
                database.Dispose();
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
            var ev = new ManualResetEventSlim();

            for (var i = 0; i < threadCount; i++)
            {
                var index = i; // capture
                threads[i] = new Thread(() =>
                {
                    var database = DatabaseContext.Database;
                    try
                    {
                        database.BeginTransaction(IsolationLevel.RepeatableRead);
                    }
                    catch (Exception e)
                    {
                        exceptions[index] = e;
                        return;
                    }
                    try
                    {
                        Console.WriteLine($"[{index}] WAIT");
                        database.AcquireLockNodeReadLock(Constants.Locks.Servers);
                        Console.WriteLine($"[{index}] GRANT");
                        lock (locker)
                        {
                            acquired++;
                            if (acquired == threadCount) ev.Set();
                        }
                        ev.Wait();
                    }
                    catch (Exception e)
                    {
                        exceptions[index] = e;
                    }
                    finally
                    {
                        Console.WriteLine($"[{index}] FREE");
                        database.CompleteTransaction();
                        database.Dispose();
                    }
                });
            }

            // need safe call context else the current one leaks into *both* threads
            using (new SafeCallContext())
            {
                foreach (var thread in threads) thread.Start();
            }

            foreach (var thread in threads) thread.Join();

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
            for (var i = 0; i < threadCount; i++)
            {
                var index = i;
                threads[i] = new Thread(() =>
                {
                    var database = DatabaseContext.Database;
                    try
                    {
                        database.BeginTransaction(IsolationLevel.RepeatableRead);
                    }
                    catch (Exception e)
                    {
                        exceptions[index] = e;
                        return;
                    }
                    try
                    {
                        Console.WriteLine($"[{index}] WAIT");
                        database.AcquireLockNodeWriteLock(Constants.Locks.Servers);
                        Console.WriteLine($"[{index}] GRANT");
                        lock (locker)
                        {
                            acquired++;
                            if (acquired > 0) throw new Exception("oops");
                        }
                        Thread.Sleep(200); // keep the log for a little while
                    }
                    finally
                    {
                        Console.WriteLine($"[{index}] FREE");
                        database.CompleteTransaction();
                        database.Dispose();
                    }
                });
            }

            // need safe call context else the current one leaks into *both* threads
            using (new SafeCallContext())
            {
                foreach (var thread in threads) thread.Start();
            }

            foreach (var thread in threads) thread.Join();

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

            // need safe call context else the current one leaks into *both* threads
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

            Assert.IsNull(e2);
        }

        private void DeadLockTestThread(int id1, int id2, EventWaitHandle myEv, WaitHandle otherEv, ref Exception exception)
        {
            var database = DatabaseContext.Database;
            try
            {
                database.BeginTransaction(IsolationLevel.RepeatableRead);
            }
            catch (Exception e)
            {
                exception = e;
                return;
            }
            try
            {
                otherEv.WaitOne();
                Console.WriteLine($"[{id1}] WAIT {id1}");
                database.AcquireLockNodeWriteLock(id1);
                Console.WriteLine($"[{id1}] GRANT {id1}");
                WriteLocks(database);
                myEv.Set();

                if (id1 == 1)
                    otherEv.WaitOne();
                else
                    Thread.Sleep(200); // cannot wait due to deadlock... just give it a bit of time

                Console.WriteLine($"[{id1}] WAIT {id2}");
                database.AcquireLockNodeWriteLock(id2);
                Console.WriteLine($"[{id1}] GRANT {id2}");
                WriteLocks(database);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{id1}] EXCEPTION {e}");
                exception = e;
            }
            finally
            {
                database.CompleteTransaction();
                database.Dispose();
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
            var database = DatabaseContext.Database;
            try
            {
                database.BeginTransaction(IsolationLevel.RepeatableRead);
            }
            catch (Exception e)
            {
                exception = e;
                return;
            }
            try
            {
                otherEv.WaitOne();
                Console.WriteLine($"[{id}] WAIT {id}");
                database.AcquireLockNodeWriteLock(id);
                Console.WriteLine($"[{id}] GRANT {id}");
                WriteLocks(database);
                myEv.Set();
                otherEv.WaitOne();
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                Console.WriteLine($"[{id}] FREE {id}");
                database.CompleteTransaction();
                database.Dispose();
                myEv.Set();
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
