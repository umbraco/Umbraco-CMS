using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;
using Umbraco.Cms.Persistence.EFCore.Locking;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Persistence.Sqlite.Interceptors;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[Timeout(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class EFCoreLockTests : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<TestUmbracoDbContext> EFScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // SQLite + retry policy makes tests fail, we retry before throwing distributed locking timeout.
        services.RemoveAll(x => !x.IsKeyedService && x.ImplementationType == typeof(SqliteAddRetryPolicyInterceptor));

        // Remove all locking implementations to ensure we only use EFCoreDistributedLockingMechanisms
        services.RemoveAll(x => x.ServiceType == typeof(IDistributedLockingMechanism));
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<TestUmbracoDbContext>>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism<TestUmbracoDbContext>>();
    }

    [SetUp]
    protected async Task SetUp()
    {
        // create a few lock objects
        using var scope = EFScopeProvider.CreateScope();
        await scope.ExecuteWithContextAsync<Task>(async database =>
        {
            database.UmbracoLocks.Add(new UmbracoLock { Id = 1, Name = "Lock.1" });
            database.UmbracoLocks.Add(new UmbracoLock { Id = 2, Name = "Lock.2" });
            database.UmbracoLocks.Add(new UmbracoLock { Id = 3, Name = "Lock.3" });

            await database.SaveChangesAsync();
        });

        scope.Complete();
    }

    [Test]
    public void SingleEagerReadLockTest()
    {
        using var scope = EFScopeProvider.CreateScope();
        scope.Locks.EagerReadLock(scope.InstanceId, Constants.Locks.Servers);
        scope.Complete();
    }

    [Test]
    public void SingleReadLockTest()
    {
        using var scope = EFScopeProvider.CreateScope();
        scope.Locks.ReadLock(scope.InstanceId, Constants.Locks.Servers);
        scope.Complete();
    }

    [Test]
    public void SingleWriteLockTest()
    {
        using var scope = EFScopeProvider.CreateScope();
        scope.Locks.WriteLock(scope.InstanceId, Constants.Locks.Servers);
        scope.Complete();
    }

    [Test]
    public void SingleEagerWriteLockTest()
    {
        using var scope = EFScopeProvider.CreateScope();
        scope.Locks.EagerWriteLock(scope.InstanceId, Constants.Locks.Servers);
        scope.Complete();
    }

    [Test]
    public void Can_Reacquire_Read_Lock()
    {
        using (var scope = EFScopeProvider.CreateScope())
        {
            scope.Locks.EagerReadLock(scope.InstanceId, Constants.Locks.Servers);
            scope.Complete();
        }

        using (var scope = EFScopeProvider.CreateScope())
        {
            scope.Locks.EagerReadLock(scope.InstanceId, Constants.Locks.Servers);
            scope.Complete();
        }
    }

    [Test]
    public void Can_Reacquire_Write_Lock()
    {
        using (var scope = EFScopeProvider.CreateScope())
        {
            scope.Locks.EagerWriteLock(scope.InstanceId, Constants.Locks.Servers);
            scope.Complete();
        }

        using (var scope = EFScopeProvider.CreateScope())
        {
            scope.Locks.EagerWriteLock(scope.InstanceId, Constants.Locks.Servers);
            scope.Complete();
        }
    }

    [Test]
    public void ConcurrentReadersTest()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            Assert.Ignore(
                "This test doesn't work with Microsoft.Data.Sqlite in EFCore as we no longer use deferred transactions");
            return;
        }

        const int threadCount = 8;
        var threads = new Thread[threadCount];
        var exceptions = new Exception[threadCount];
        Lock locker = new();
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
                        scope.Locks.EagerReadLock(scope.InstanceId, Constants.Locks.Servers);
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
    public void ConcurrentWritersTest()
    {
        if (BaseTestDatabase.IsSqlite())
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

                        scope.Locks.EagerWriteLock(scope.InstanceId, Constants.Locks.Servers);

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
        Thread.Sleep(1000);
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

        Assert.IsNotNull(e1);
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
        using var scope = EFScopeProvider.CreateScope();
        try
        {
            otherEv.WaitOne();
            Console.WriteLine($"[{id1}] WAIT {id1}");
            scope.Locks.EagerWriteLock(scope.InstanceId, id1);
            Console.WriteLine($"[{id1}] GRANT {id1}");
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
            scope.Locks.EagerWriteLock(scope.InstanceId, id2);
            Console.WriteLine($"[{id1}] GRANT {id2}");
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
