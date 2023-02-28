using System.Text;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Persistence.Sqlite.Interceptors;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore;

[TestFixture]
[Timeout(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class EfCoreLockTests : UmbracoIntegrationTest
{
    private IEfCoreScopeProvider EFScopeProvider =>
        GetRequiredService<IEfCoreScopeProvider>();

    private EFCoreScopeAccessor EFScopeAccessor => (EFCoreScopeAccessor)GetRequiredService<IEFCoreScopeAccessor>();

    [SetUp]
    protected void SetUp()
    {
        // create a few lock objects
        using (var scope = EFScopeProvider.CreateScope())
        {
            scope.ExecuteWithContextAsync<Task>(async database =>
            {
                database.UmbracoLocks.Add(new UmbracoLock {Id = 1, Name = "Lock.1"});
                database.UmbracoLocks.Add(new UmbracoLock {Id = 2, Name = "Lock.2"});
                database.UmbracoLocks.Add(new UmbracoLock {Id = 3, Name = "Lock.3"});
            });

            scope.Complete();
        }
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // SQLite + retry policy makes tests fail, we retry before throwing distributed locking timeout.
        services.RemoveAll(x => x.ImplementationType == typeof(SqliteAddRetryPolicyInterceptor));
        services.AddUnique<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism>();
    }


    [Test]
    public void SingleReadLockTest()
    {
        using (var scope = EFScopeProvider.CreateScope())
        {
            scope.EagerReadLock(Constants.Locks.Servers);
            scope.Complete();
        }
    }

    [Test]
    public void ConcurrentReadersTest()
    {
        if (BaseTestDatabase.DatabaseType.IsSqlite())
        {
            Assert.Ignore(
                "This test doesn't work with Microsoft.Data.Sqlite in EFCore as we no longer use deferred transactions");
            return;
        }

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
                using (var scope = EFScopeProvider.CreateScope())
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

        using (var scope = EFScopeProvider.CreateScope())
        {
            // Issue a lock request, but we are using non-eager
            // locks so this only queues the request.
            // The lock will not be issued unless we resolve
            // scope.Database
            scope.WriteLock(Constants.Locks.Servers);

            scope.ExecuteWithContextAsync<Task>(async db =>
            {
                sqlCount = db.ChangeTracker.Entries().Count();
            });
        }

        Assert.AreEqual(0, sqlCount);
    }

    [Test]
    public void ConcurrentWritersTest()
    {
        if (BaseTestDatabase.DatabaseType.IsSqlite())
        {
            Assert.Ignore(
                "This test doesn't work with Microsoft.Data.Sqlite in EFCore as we no longer use deferred transactions");
            return;
        }

        const int threadCount = 3;
        var threads = new Thread[threadCount];
        var exceptions = new Exception[threadCount];
        var locker = new object();
        var acquired = 0;
        int triedAcquiringWriteLock = 0;
        var entered = 0;
        var ms = new AutoResetEvent[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            ms[i] = new AutoResetEvent(false);
        }

        var m1 = new ManualResetEventSlim(false);
        var m2 = new ManualResetEventSlim(false);

        for (var i = 0; i < threadCount; i++)
        {
            var ic = i; // capture
            threads[i] = new Thread(() =>
            {
                using (var scope = EFScopeProvider.CreateScope())
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

                        lock (locker)
                        {
                            triedAcquiringWriteLock++;
                            if (triedAcquiringWriteLock == threadCount)
                            {
                                m2.Set();
                            }
                        }

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

        m2.Wait();
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
        if (BaseTestDatabase.DatabaseType.IsSqlite())
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
        using (var scope = EFScopeProvider.CreateScope())
        {
            try
            {
                otherEv.WaitOne();
                Console.WriteLine($"[{id1}] WAIT {id1}");
                scope.EagerWriteLock(id1);
                Console.WriteLine($"[{id1}] GRANT {id1}");
                WriteLocks(scope);
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
                WriteLocks(scope);
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

    private void WriteLocks(IEfCoreScope scope)
    {
        Console.WriteLine("LOCKS:");
        List<dynamic> info = new List<dynamic>();
        scope.ExecuteWithContextAsync<Task>(async db =>
        {
            info = await db.Database.ExecuteScalarAsync<List<dynamic>>("SELECT * FROM sys.dm_tran_locks;");
        });

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
