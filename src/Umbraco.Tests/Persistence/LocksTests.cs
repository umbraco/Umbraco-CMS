using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;
using Umbraco.Tests.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    // fixme [Ignore("Takes too much time.")]
    public class LocksTests : TestWithDatabaseBase
    {
        //private ThreadSafetyServiceTest.PerThreadPetaPocoUnitOfWorkProvider _uowProvider;
        //private ThreadSafetyServiceTest.PerThreadDatabaseFactory _dbFactory;

        public override void SetUp()
        {
            base.SetUp();

            // need to use a special IDatabaseFactory because we MUST ensure that a Database instance 
            // is created per thread - though... now the default one is LCC based, should it work? fixme
            Container.Register<IDatabaseFactory, ThreadSafetyServiceTest.PerThreadSqlCeDatabaseFactory>();

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
        public void Test()
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
            }
        }

        [Test]
        public void ConcurrentReadersTest()
        {
            var threads = new List<Thread>();
            var locker = new object();
            var acquired = 0;
            var maxAcquired = 0;
            for (var i = 0; i < 5; i++)
            {
                threads.Add(new Thread(() =>
                {
                    var database = DatabaseContext.Database;
                    database.BeginTransaction(IsolationLevel.RepeatableRead);
                    try
                    {
                        database.AcquireLockNodeReadLock(Constants.Locks.Servers);
                        lock (locker)
                        {
                            acquired++;
                        }
                        Thread.Sleep(500);
                        lock (locker)
                        {
                            if (maxAcquired < acquired) maxAcquired = acquired;
                        }
                        Thread.Sleep(500);
                        lock (locker)
                        {
                            acquired--;
                        }
                    }
                    finally
                    {
                        database.CompleteTransaction();
                    }
                }));
            }
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            Assert.AreEqual(5, maxAcquired);
        }

        [Test]
        public void ConcurrentWritersTest()
        {
            var threads = new List<Thread>();
            var locker = new object();
            var acquired = 0;
            var maxAcquired = 0;
            for (var i = 0; i < 5; i++)
            {
                threads.Add(new Thread(() =>
                {
                    var database = DatabaseContext.Database;
                    database.BeginTransaction(IsolationLevel.RepeatableRead);
                    try
                    {
                        database.AcquireLockNodeWriteLock(Constants.Locks.Servers);
                        lock (locker)
                        {
                            acquired++;
                        }
                        Thread.Sleep(500);
                        lock (locker)
                        {
                            if (maxAcquired < acquired) maxAcquired = acquired;
                        }
                        Thread.Sleep(500);
                        lock (locker)
                        {
                            acquired--;
                        }
                    }
                    finally
                    {
                        database.CompleteTransaction();
                    }
                }));
            }
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
            Assert.AreEqual(1, maxAcquired);
        }

        [Test]
        public void DeadLockTest()
        {
            Exception e1 = null, e2 = null;

            var thread1 = new Thread(() =>
            {
                var database = DatabaseContext.Database;
                database.BeginTransaction(IsolationLevel.RepeatableRead);
                try
                {
                    database.AcquireLockNodeWriteLock(1);
                    Thread.Sleep(1000);
                    database.AcquireLockNodeWriteLock(2);
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    e1 = e;
                }
                finally
                {
                    database.CompleteTransaction();
                }
            });
            var thread2 = new Thread(() =>
            {
                var database = DatabaseContext.Database;
                database.BeginTransaction(IsolationLevel.RepeatableRead);
                try
                {
                    database.AcquireLockNodeWriteLock(2);
                    Thread.Sleep(1000);
                    database.AcquireLockNodeWriteLock(1);
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    e2 = e;
                }
                finally
                {
                    database.CompleteTransaction();
                }
            });
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();
            Assert.IsNotNull(e1);
            Assert.IsNotNull(e2);
            Assert.IsInstanceOf<SqlCeLockTimeoutException>(e1);
            Assert.IsInstanceOf<SqlCeLockTimeoutException>(e2);
        }

        [Test]
        public void NoDeadLockTest()
        {
            Exception e1 = null, e2 = null;

            var thread1 = new Thread(() =>
            {
                var database = DatabaseContext.Database;
                database.BeginTransaction(IsolationLevel.RepeatableRead);
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
                finally
                {
                    database.CompleteTransaction();
                }
            });
            var thread2 = new Thread(() =>
            {
                var database = DatabaseContext.Database;
                database.BeginTransaction(IsolationLevel.RepeatableRead);
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
                finally
                {
                    database.CompleteTransaction();
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
