using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Persistence.Sqlite.Interceptors;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

[TestFixture]
[Timeout(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class LocksTests : UmbracoIntegrationTest
{
    [SetUp]
    protected void SetUp()
    {
        // create a few lock objects
        using (var scope = ScopeProvider.CreateScope())
        {
            var database = ScopeAccessor.AmbientScope.Database;
            database.Insert("umbracoLock", "id", false, new LockDto { Id = 1, Name = "Lock.1" });
            database.Insert("umbracoLock", "id", false, new LockDto { Id = 2, Name = "Lock.2" });
            database.Insert("umbracoLock", "id", false, new LockDto { Id = 3, Name = "Lock.3" });
            scope.Complete();
        }
    }

    protected override void ConfigureTestServices(IServiceCollection services) =>
        // SQLite + retry policy makes tests fail, we retry before throwing distributed locking timeout.
        services.RemoveAll(x => x.ImplementationType == typeof(SqliteAddRetryPolicyInterceptor));

    [Test]
    public void SingleReadLockTest()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.EagerReadLock(Constants.Locks.Servers);
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
                        scope.EagerReadLock(Constants.Locks.Servers);
                        lock (locker)
                        {
                            acquired++;
                            if (acquired == threadCount)
                            {
                                m2.Set();
                            }
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

        // ensure that current scope does not leak into starting threads
        using (ExecutionContext.SuppressFlow())
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        m2.Wait();
        // all threads have locked in parallel
        var maxAcquired = acquired;
        m1.Set();

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.AreEqual(threadCount, maxAcquired);
        Assert.AreEqual(0, acquired);

        for (var i = 0; i < threadCount; i++)
        {
            Assert.IsNull(exceptions[i]);
        }
    }

    [Test]
    public void GivenNonEagerLocking_WhenNoDbIsAccessed_ThenNoSqlIsExecuted()
    {
        var sqlCount = 0;

        using (var scope = ScopeProvider.CreateScope())
        {
            var db = ScopeAccessor.AmbientScope.Database;
            try
            {
                db.EnableSqlCount = true;

                // Issue a lock request, but we are using non-eager
                // locks so this only queues the request.
                // The lock will not be issued unless we resolve
                // scope.Database
                scope.WriteLock(Constants.Locks.Servers);

                sqlCount = db.SqlCount;
            }
            finally
            {
                db.EnableSqlCount = false;
            }
        }

        Assert.AreEqual(0, sqlCount);
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
        for (var i = 0; i < threadCount; i++)
        {
            ms[i] = new AutoResetEvent(false);
        }

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
                            if (entered == threadCount)
                            {
                                m1.Set();
                            }
                        }

                        ms[ic].WaitOne();
                        scope.EagerWriteLock(Constants.Locks.Servers);
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

        // ensure that current scope does not leak into starting threads
        using (ExecutionContext.SuppressFlow())
        {
            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        m1.Wait();
        // all threads have entered
        ms[0].Set(); // let 0 go
        // TODO: This timing is flaky
        Thread.Sleep(100);
        for (var i = 1; i < threadCount; i++)
        {
            ms[i].Set(); // let others go
        }

        // TODO: This timing is flaky
        Thread.Sleep(500);
        // only 1 thread has locked
        Assert.AreEqual(1, acquired);
        for (var i = 0; i < threadCount; i++)
        {
            ms[i].Set(); // let all go
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.AreEqual(0, acquired);

        for (var i = 0; i < threadCount; i++)
        {
            Assert.IsNull(exceptions[i]);
        }
    }

    [Retry(10)] // TODO make this test non-flaky.
    [Test]
    public void DeadLockTest()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            Assert.Ignore("This test doesn't work with Microsoft.Data.Sqlite - SELECT * FROM sys.dm_tran_locks;");
            return;
        }

        Exception e1 = null, e2 = null;
        AutoResetEvent ev1 = new(false), ev2 = new(false);

        // testing:
        // two threads will each obtain exclusive write locks over two
        // identical lock objects deadlock each other

        var thread1 = new Thread(() => DeadLockTestThread(1, 2, ev1, ev2, ref e1));
        var thread2 = new Thread(() => DeadLockTestThread(2, 1, ev2, ev1, ref e2));

        // ensure that current scope does not leak into starting threads
        using (ExecutionContext.SuppressFlow())
        {
            thread1.Start();
            thread2.Start();
        }

        ev2.Set();

        thread1.Join();
        thread2.Join();

        //Assert.IsNotNull(e1);
        if (e1 != null)
        {
            AssertIsDistributedLockingTimeoutException(e1);
        }

        // the assertion below depends on timing conditions - on a fast enough environment,
        // thread1 dies (deadlock) and frees thread2, which succeeds - however on a slow
        // environment (CI) both threads can end up dying due to deadlock - so, cannot test
        // that e2 is null - but if it's not, can test that it's a timeout
        //
        //Assert.IsNull(e2);
        if (e2 != null)
        {
            AssertIsDistributedLockingTimeoutException(e2);
        }
    }

    private void AssertIsDistributedLockingTimeoutException(Exception e)
    {
        var sqlException = e as DistributedLockingTimeoutException;
        Assert.IsNotNull(sqlException);
    }

    private void DeadLockTestThread(int id1, int id2, EventWaitHandle myEv, WaitHandle otherEv, ref Exception exception)
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            try
            {
                otherEv.WaitOne();
                Console.WriteLine($"[{id1}] WAIT {id1}");
                scope.EagerWriteLock(id1);
                Console.WriteLine($"[{id1}] GRANT {id1}");
                WriteLocks(ScopeAccessor.AmbientScope.Database);
                myEv.Set();

                if (id1 == 1)
                {
                    otherEv.WaitOne();
                }
                else
                {
                    Thread.Sleep(5200); // wait for deadlock...
                }

                Console.WriteLine($"[{id1}] WAIT {id2}");
                scope.EagerWriteLock(id2);
                Console.WriteLine($"[{id1}] GRANT {id2}");
                WriteLocks(ScopeAccessor.AmbientScope.Database);
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
        if (BaseTestDatabase.IsSqlite())
        {
            Assert.Ignore("This test doesn't work with Microsoft.Data.Sqlite - SELECT * FROM sys.dm_tran_locks;");
            return;
        }


        Exception e1 = null, e2 = null;
        AutoResetEvent ev1 = new(false), ev2 = new(false);

        // testing:
        // two threads will each obtain exclusive write lock over two
        // different lock objects without blocking each other

        var thread1 = new Thread(() => NoDeadLockTestThread(1, ev1, ev2, ref e1));
        var thread2 = new Thread(() => NoDeadLockTestThread(2, ev2, ev1, ref e1));

        // ensure that current scope does not leak into starting threads
        using (ExecutionContext.SuppressFlow())
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

    [Test]
    public void Throws_When_Lock_Timeout_Is_Exceeded_Read()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            // Reader reads snapshot, isolated from the writer.
            Assert.Ignore("Doesn't apply to SQLite with journal_mode=wal");
        }

        using (ExecutionContext.SuppressFlow())
        {
            var t1 = Task.Run(() =>
            {
                using (var scope = ScopeProvider.CreateScope())
                {
                    Console.WriteLine("Write lock A");
                    // This will acquire right away
                    scope.EagerWriteLock(TimeSpan.FromMilliseconds(2000), Constants.Locks.ContentTree);
                    Thread.Sleep(6000); // Wait longer than the Read Lock B timeout
                    scope.Complete();
                    Console.WriteLine("Finished Write lock A");
                }
            });

            Thread.Sleep(500); // 100% sure task 1 starts first

            var t2 = Task.Run(() =>
            {
                using (var scope = ScopeProvider.CreateScope())
                {
                    Console.WriteLine("Read lock B");

                    // This will wait for the write lock to release but it isn't going to wait long
                    // enough so an exception will be thrown.
                    Assert.Throws<DistributedReadLockTimeoutException>(() =>
                        scope.EagerReadLock(TimeSpan.FromMilliseconds(3000), Constants.Locks.ContentTree));
                    scope.Complete();
                    Console.WriteLine("Finished Read lock B");
                }
            });

            Task.WaitAll(t1, t2);
        }
    }

    [Test]
    public void Throws_When_Lock_Timeout_Is_Exceeded_Write()
    {
        var counter = 0;
        var gate = new ManualResetEventSlim(false);
        var logger = GetRequiredService<ILogger<LocksTests>>();

        using (ExecutionContext.SuppressFlow())
        {
            var t1 = Task.Run(() =>
            {
                using var scope = ScopeProvider.CreateScope();

                _ = scope.Database; // Begin transaction
                Interlocked.Increment(ref counter);
                gate.Wait();

                logger.LogInformation("t1 - Attempting to acquire write lock");
                Assert.DoesNotThrow(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    // This will acquire right away
                    scope.EagerWriteLock(TimeSpan.FromMilliseconds(1000), Constants.Locks.ContentTree);
                });

                logger.LogInformation("t1 - Acquired write lock, sleeping");
                Thread.Sleep(1500); // Wait longer than the Read Lock B timeout

                scope.Complete();
                logger.LogInformation("t1 -  Complete transaction");
            });

            var t2 = Task.Run(() =>
            {
                using var scope = ScopeProvider.CreateScope();

                _ = scope.Database; // Begin transaction
                Interlocked.Increment(ref counter);
                gate.Wait();
                Thread.Sleep(100); // Let other transaction obtain write lock first.

                logger.LogInformation("t2 - Attempting to acquire write lock");
                var ex = Assert.Throws<DistributedWriteLockTimeoutException>(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    scope.EagerWriteLock(TimeSpan.FromMilliseconds(1000), Constants.Locks.ContentTree);
                    logger.LogInformation("t2 - Acquired write lock, something has gone wrong.");
                });

                if (ex != null)
                {
                    logger.LogInformation("t2 - Failed to acquire write lock in time, all is well.");
                }

                scope.Complete();
            });

            while (counter < 2)
            {
                Thread.Sleep(10);
            }

            gate.Set();
            Task.WaitAll(t1, t2);
        }
    }

    [Test]
    public void Read_Lock_Waits_For_Write_Lock()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            // Reader reads snapshot, isolated from the writer.
            Assert.Ignore("Doesn't apply to SQLite with journal_mode=wal");
        }

        var locksCompleted = 0;

        using (ExecutionContext.SuppressFlow())
        {
            var t1 = Task.Run(() =>
            {
                using (var scope = ScopeProvider.CreateScope())
                {
                    Console.WriteLine("Write lock A");
                    // This will acquire right away
                    scope.EagerWriteLock(TimeSpan.FromMilliseconds(2000), Constants.Locks.ContentTree);
                    Thread.Sleep(4000); // Wait less than the Read Lock B timeout
                    scope.Complete();
                    Interlocked.Increment(ref locksCompleted);
                    Console.WriteLine("Finished Write lock A");
                }
            });

            Thread.Sleep(500); // 100% sure task 1 starts first

            var t2 = Task.Run(() =>
            {
                using (var scope = ScopeProvider.CreateScope())
                {
                    Console.WriteLine("Read lock B");

                    // This will wait for the write lock to release
                    Assert.DoesNotThrow(() =>
                        scope.EagerReadLock(TimeSpan.FromMilliseconds(6000), Constants.Locks.ContentTree));

                    Assert.GreaterOrEqual(locksCompleted, 1);

                    scope.Complete();
                    Interlocked.Increment(ref locksCompleted);
                    Console.WriteLine("Finished Read lock B");
                }
            });


            var t3 = Task.Run(() =>
            {
                using (var scope = ScopeProvider.CreateScope())
                {
                    Console.WriteLine("Read lock C");

                    // This will wait for the write lock to release
                    Assert.DoesNotThrow(() =>
                        scope.EagerReadLock(TimeSpan.FromMilliseconds(6000), Constants.Locks.ContentTree));

                    Assert.GreaterOrEqual(locksCompleted, 1);

                    scope.Complete();
                    Interlocked.Increment(ref locksCompleted);
                    Console.WriteLine("Finished Read lock C");
                }
            });

            Task.WaitAll(t1, t2, t3);
        }

        Assert.AreEqual(3, locksCompleted);
    }

    [Test]
    public void Lock_Exceeds_Command_Timeout()
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            var realDb = (Database)ScopeAccessor.AmbientScope.Database;
            realDb.CommandTimeout = 1000;

            Console.WriteLine("Write lock A");
            // TODO: In theory this would throw
            scope.EagerWriteLock(TimeSpan.FromMilliseconds(3000), Constants.Locks.ContentTree);
            scope.Complete();
            Console.WriteLine("Finished Write lock A");
        }
    }


    private void NoDeadLockTestThread(int id, EventWaitHandle myEv, WaitHandle otherEv, ref Exception exception)
    {
        using (var scope = ScopeProvider.CreateScope())
        {
            try
            {
                otherEv.WaitOne();
                Console.WriteLine($"[{id}] WAIT {id}");
                scope.EagerWriteLock(id);
                Console.WriteLine($"[{id}] GRANT {id}");
                WriteLocks(ScopeAccessor.AmbientScope.Database);
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
        var info = database.Query<dynamic>("SELECT * FROM sys.dm_tran_locks;").ToList();
        var sb = new StringBuilder("> ");
        foreach (var row in info)
        {
            if (row is IDictionary<string, object> values)
            {
                sb.AppendJoin(", ", values);
            }

            sb.AppendLine(string.Empty);
        }

        Console.WriteLine(sb.ToString());
    }
}
