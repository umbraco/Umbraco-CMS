using NUnit.Framework;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
public class EfCoreScopeTest : UmbracoIntegrationTest
{
    private IEfCoreScopeProvider EfCoreScopeProvider =>
        GetRequiredService<IEfCoreScopeProvider>();

    private IEFCoreScopeAccessor EfCoreScopeAccessor => GetRequiredService<IEFCoreScopeAccessor>();

    [Test]
    public void CanCreateScope()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EfCoreScope>(scope);
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
    }

    [Test]
    public void CanCreateScopeTwice() =>
        Assert.DoesNotThrow(() =>
        {
            using (var scope = EfCoreScopeProvider.CreateScope())
            {
                scope.Complete();
            }

            using (var scopeTwo = EfCoreScopeProvider.CreateScope())
            {
                scopeTwo.Complete();
            }
        });

    [Test]
    public void NestedCreateScope()
    {

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EfCoreScope>(scope);
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
            using (IEfCoreScope nested = EfCoreScopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<EfCoreScope>(nested);
                Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
                Assert.AreSame(nested, EfCoreScopeAccessor.AmbientScope);
                Assert.AreSame(scope, ((EfCoreScope)nested).ParentScope);
            }
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
    }

    [Test]
    public async Task CanAccessDbContext()
    {
        using var scope = EfCoreScopeProvider.CreateScope();
        await scope.ExecuteWithContextAsync(async db =>
        {
            Assert.IsTrue(await db.Database.CanConnectAsync());
            Assert.IsNotNull(db.Database.CurrentTransaction); // in a transaction
            return Task.CompletedTask;
        });
        scope.Complete();
    }

    [Test]
    public async Task CanAccessNestedDbContext()
    {
        using var scope = EfCoreScopeProvider.CreateScope();
        await scope.ExecuteWithContextAsync(async db =>
        {
            Assert.IsTrue(await db.Database.CanConnectAsync());
            var parentTransaction = db.Database.CurrentTransaction;

            using (var nestedSCope = EfCoreScopeProvider.CreateScope())
            {
                await nestedSCope.ExecuteWithContextAsync(async nestedDb =>
                {
                    Assert.IsTrue(await nestedDb.Database.CanConnectAsync());
                    Assert.IsNotNull(nestedDb.Database.CurrentTransaction); // in a transaction
                    var childTransaction = nestedDb.Database.CurrentTransaction;
                    Assert.AreSame(parentTransaction, childTransaction);
                    return Task.CompletedTask;
                });
            }

            return Task.CompletedTask;
        });
        scope.Complete();
    }

    [Test]
    public void GivenUncompletedScopeOnChildThread_WhenTheParentCompletes_TheTransactionIsRolledBack()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope mainScope = EfCoreScopeProvider.CreateScope();

        var t = Task.Run(() =>
        {
            IEfCoreScope nested = EfCoreScopeProvider.CreateScope();
            Thread.Sleep(2000);
            nested.Dispose();
        });

        Thread.Sleep(1000); // mimic some long running operation that is shorter than the other thread
        mainScope.Complete();
        Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());

        Task.WaitAll(t);
    }

    [Test]
    public void GivenNonDisposedChildScope_WhenTheParentDisposes_ThenInvalidOperationExceptionThrows()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope mainScope = EfCoreScopeProvider.CreateScope();

        IEfCoreScope nested = EfCoreScopeProvider.CreateScope(); // not disposing

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
        Console.WriteLine(ex);
    }

    [Test]
    public void GivenChildThread_WhenParentDisposedBeforeChild_ParentScopeThrows()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope mainScope = EfCoreScopeProvider.CreateScope();

        var t = Task.Run(() =>
        {
            Console.WriteLine("Child Task start: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);

            // This will push the child scope to the top of the Stack
            IEfCoreScope nested = EfCoreScopeProvider.CreateScope();
            Console.WriteLine("Child Task scope created: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
            Thread.Sleep(5000); // block for a bit to ensure the parent task is disposed first
            Console.WriteLine("Child Task before dispose: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
            nested.Dispose();
            Console.WriteLine("Child Task after dispose: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
        });

        // provide some time for the child thread to start so the ambient context is copied in AsyncLocal
        Thread.Sleep(2000);

        // now dispose the main without waiting for the child thread to join
        Console.WriteLine("Parent Task disposing: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);

        // This will throw because at this stage a child scope has been created which means
        // it is the Ambient (top) scope but here we're trying to dispose the non top scope.
        Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
        t.Wait();        // wait for the child to dispose
        mainScope.Dispose();    // now it's ok
        Console.WriteLine("Parent Task disposed: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
    }

    [Test]
    public void GivenChildThread_WhenChildDisposedBeforeParent_OK()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope mainScope = EfCoreScopeProvider.CreateScope();

        // Task.Run will flow the execution context unless ExecutionContext.SuppressFlow() is explicitly called.
        // This is what occurs in normal async behavior since it is expected to await (and join) the main thread,
        // but if Task.Run is used as a fire and forget thread without being done correctly then the Scope will
        // flow to that thread.
        var t = Task.Run(() =>
        {
            Console.WriteLine("Child Task start: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
            IEfCoreScope nested = EfCoreScopeProvider.CreateScope();
            Console.WriteLine("Child Task before dispose: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
            nested.Dispose();
            Console.WriteLine("Child Task after disposed: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
        });

        Console.WriteLine("Parent Task waiting: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
        t.Wait();
        Console.WriteLine("Parent Task disposing: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
        mainScope.Dispose();
        Console.WriteLine("Parent Task disposed: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);

        Assert.Pass();
    }

}
