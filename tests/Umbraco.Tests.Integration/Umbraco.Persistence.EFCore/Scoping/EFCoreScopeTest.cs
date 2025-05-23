using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class EFCoreScopeTest : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<TestUmbracoDbContext> EfCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    private EFCoreScopeAccessor<TestUmbracoDbContext> EfCoreScopeAccessor => (EFCoreScopeAccessor<TestUmbracoDbContext>)GetRequiredService<IEFCoreScopeAccessor<TestUmbracoDbContext>>();

    [Test]
    public void CanCreateScope()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
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
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
            using (IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(nested);
                Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
                Assert.AreSame(nested, EfCoreScopeAccessor.AmbientScope);
                Assert.AreSame(scope, ((EFCoreScope<TestUmbracoDbContext>)nested).ParentScope);
            }
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
    }

    [Test]
    public async Task NestedCreateScopeInnerException()
    {
        bool scopeCompleted = false;

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        try
        {
            using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
            {
                // scopeProvider.Context.Enlist("test", completed => scopeCompleted = completed);
                await scope.ExecuteWithContextAsync(database =>
                {
                    scope.ScopeContext!.Enlist("test", completed => scopeCompleted = completed);
                    Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
                    Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
                    Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
                    using (IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope())
                    {
                        Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(nested);
                        Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
                        Assert.AreSame(nested, EfCoreScopeAccessor.AmbientScope);
                        Assert.AreSame(scope, ((EFCoreScope<TestUmbracoDbContext>)nested).ParentScope);
                        nested.Complete();
                        throw new Exception("bang!");
                    }

                    return Task.FromResult(true);
                });

                scope.Complete();
            }

            Assert.Fail("Expected exception.");
        }
        catch (Exception e)
        {
            if (e.Message != "bang!")
            {
                Assert.Fail("Wrong exception.");
            }
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        Assert.IsFalse(scopeCompleted);
    }

    [Test]
    public async Task CanAccessDbContext()
    {
        using var scope = EfCoreScopeProvider.CreateScope();
        await scope.ExecuteWithContextAsync<Task>(async database =>
        {
            Assert.IsTrue(await database.Database.CanConnectAsync());
            Assert.IsNotNull(database.Database.CurrentTransaction); // in a transaction
        });
        scope.Complete();
    }

    [Test]
    public async Task CanAccessDbContextTwice()
    {
        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                Assert.IsTrue(await database.Database.CanConnectAsync());
                Assert.IsNotNull(database.Database.CurrentTransaction); // in a transaction
            });
            scope.Complete();
        }

        using (var scopeTwo = EfCoreScopeProvider.CreateScope())
        {
            await scopeTwo.ExecuteWithContextAsync<Task>(async database =>
            {
                Assert.IsTrue(await database.Database.CanConnectAsync());
                Assert.IsNotNull(database.Database.CurrentTransaction); // in a transaction
            });

            scopeTwo.Complete();
        }
    }

    [Test]
    public async Task CanAccessNestedDbContext()
    {
        using (var scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                Assert.IsTrue(await database.Database.CanConnectAsync());
                var parentTransaction = database.Database.CurrentTransaction;

                using (var nestedSCope = EfCoreScopeProvider.CreateScope())
                {
                    await nestedSCope.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        Assert.IsTrue(await nestedDatabase.Database.CanConnectAsync());
                        Assert.IsNotNull(nestedDatabase.Database.CurrentTransaction); // in a transaction
                        var childTransaction = nestedDatabase.Database.CurrentTransaction;
                        Assert.AreSame(parentTransaction, childTransaction);
                    });
                }
            });
            scope.Complete();
        }
    }

    [Test]
    public void GivenUncompletedScopeOnChildThread_WhenTheParentCompletes_TheTransactionIsRolledBack()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope<TestUmbracoDbContext> mainScope = EfCoreScopeProvider.CreateScope();

        var t = Task.Run(() =>
        {
            IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope();
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
        IEfCoreScope<TestUmbracoDbContext> mainScope = EfCoreScopeProvider.CreateScope();

        IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope(); // not disposing

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
        Console.WriteLine(ex);
    }

    [Test]
    public void GivenChildThread_WhenParentDisposedBeforeChild_ParentScopeThrows()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope<TestUmbracoDbContext> mainScope = EfCoreScopeProvider.CreateScope();

        var t = Task.Run(() =>
        {
            Console.WriteLine("Child Task start: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);

            // This will push the child scope to the top of the Stack
            IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope();
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
        t.Wait(); // wait for the child to dispose
        mainScope.Dispose(); // now it's ok
        Console.WriteLine("Parent Task disposed: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
    }

    [Test]
    public void GivenChildThread_WhenChildDisposedBeforeParent_OK()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        IEfCoreScope<TestUmbracoDbContext> mainScope = EfCoreScopeProvider.CreateScope();

        // Task.Run will flow the execution context unless ExecutionContext.SuppressFlow() is explicitly called.
        // This is what occurs in normal async behavior since it is expected to await (and join) the main thread,
        // but if Task.Run is used as a fire and forget thread without being done correctly then the Scope will
        // flow to that thread.
        var t = Task.Run(() =>
        {
            Console.WriteLine("Child Task start: " + EfCoreScopeAccessor.AmbientScope?.InstanceId);
            IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope();
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

    [Test]
    public async Task Transaction()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(n);
            });
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
            });

            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            });

            scope.Complete();
        }
    }

    [Test]
    public async Task NestedTransactionInnerFail()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp1 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            string n;
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp1 (id, name) VALUES (1, 'a')");
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope())
                {
                    await nested.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        await nestedDatabase.Database.ExecuteSqlAsync($"INSERT INTO tmp1 (id, name) VALUES (2, 'b')");
                        string nn = await nestedDatabase.Database.ExecuteScalarAsync<string>(
                            "SELECT name FROM tmp1 WHERE id=2");
                        Assert.AreEqual("b", nn);
                    });
                }

                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.AreEqual("b", n);
            });

            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.IsNull(n);
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.IsNull(n);
            });
        }
    }

    [Test]
    public async Task NestedTransactionOuterFail()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp2 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp2 (id, name) VALUES (1, 'a')");
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope())
                {
                    await scope.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        await nestedDatabase.Database.ExecuteSqlAsync($"INSERT INTO tmp2 (id, name) VALUES (2, 'b')");
                        string nn = await nestedDatabase.Database.ExecuteScalarAsync<string>(
                            "SELECT name FROM tmp2 WHERE id=2");
                        Assert.AreEqual("b", nn);
                    });

                    nested.Complete();
                }

                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.AreEqual("b", n);
            });
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.IsNull(n);
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.IsNull(n);
            });
        }
    }

    [Test]
    public async Task NestedTransactionComplete()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp (id, name) VALUES (1, 'a')");
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);

                using (IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope())
                {
                    await scope.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        await nestedDatabase.Database.ExecuteSqlAsync($"INSERT INTO tmp (id, name) VALUES (2, 'b')");
                        string nn =
                            await nestedDatabase.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=2");
                        Assert.AreEqual("b", nn);
                    });

                    nested.Complete();
                }

                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
            });

            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
            });
        }
    }

    [Test]
    public void CallContextScope1()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);

            // Run on another thread without a flowed context
            Task t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.IsNull(EfCoreScopeAccessor.AmbientScope);

                using (IEfCoreScope<TestUmbracoDbContext> newScope = EfCoreScopeProvider.CreateScope())
                {
                    Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
                    Assert.IsNull(EfCoreScopeAccessor.AmbientScope.ParentScope);
                }

                Assert.IsNull(EfCoreScopeAccessor.AmbientScope);

                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
    }

    [Test]
    public void CallContextScope2()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);

        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);

            // Run on another thread without a flowed context
            Task t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.IsNull(EfCoreScopeAccessor.AmbientScope);

                using (IEfCoreScope<TestUmbracoDbContext> newScope = EfCoreScopeProvider.CreateScope())
                {
                    Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
                    Assert.IsNull(EfCoreScopeAccessor.AmbientScope.ParentScope);
                }

                Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ScopeContextEnlist(bool complete)
    {
        bool? completed = null;
        IEfCoreScope<TestUmbracoDbContext> ambientScope = null;
        IScopeContext ambientContext = null;

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            scope.ScopeContext.Enlist("name", c =>
            {
                completed = c;
                ambientScope = EfCoreScopeAccessor.AmbientScope;
                ambientContext = EfCoreScopeProvider.AmbientScopeContext;
            });
            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        Assert.IsNull(EfCoreScopeProvider.AmbientScopeContext);
        Assert.IsNotNull(completed);
        Assert.AreEqual(complete, completed.Value);
        Assert.IsNull(ambientScope); // the scope is gone
        Assert.IsNotNull(ambientContext); // the context is still there
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ScopeContextEnlistAgain(bool complete)
    {
        bool? completed = null;
        bool? completed2 = null;

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            scope.ScopeContext.Enlist("name", c =>
            {
                completed = c;

                // at that point the scope is gone, but the context is still there
                IScopeContext ambientContext = EfCoreScopeProvider.AmbientScopeContext;
                ambientContext.Enlist("another", c2 => completed2 = c2);
            });
            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        Assert.IsNull(EfCoreScopeProvider.AmbientScopeContext);
        Assert.IsNotNull(completed);
        Assert.AreEqual(complete, completed.Value);
        Assert.AreEqual(complete, completed2.Value);
    }

    [Test]
    public void DetachableScope()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);

            Assert.IsNotNull(EfCoreScopeProvider.AmbientScopeContext); // the ambient context
            Assert.IsNotNull(scope.ScopeContext); // the ambient context too (getter only)
            IScopeContext context = scope.ScopeContext;

            IEfCoreScope<TestUmbracoDbContext> detached = EfCoreScopeProvider.CreateDetachedScope();
            EfCoreScopeProvider.AttachScope(detached);

            Assert.AreEqual(detached, EfCoreScopeAccessor.AmbientScope);
            Assert.AreNotSame(context, EfCoreScopeProvider.AmbientScopeContext);

            // nesting under detached!
            using (IEfCoreScope<TestUmbracoDbContext> nested = EfCoreScopeProvider.CreateScope())
            {
                Assert.Throws<InvalidOperationException>(() =>

                    // cannot detach a non-detachable scope
                    EfCoreScopeProvider.DetachScope());
                nested.Complete();
            }

            Assert.AreEqual(detached, EfCoreScopeAccessor.AmbientScope);
            Assert.AreNotSame(context, EfCoreScopeProvider.AmbientScopeContext);

            // can detach
            Assert.AreSame(detached, EfCoreScopeProvider.DetachScope());

            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(context, EfCoreScopeProvider.AmbientScopeContext);

            Assert.Throws<InvalidOperationException>(() =>

                // cannot disposed a non-attached scope
                // in fact, only the ambient scope can be disposed
                detached.Dispose());

            EfCoreScopeProvider.AttachScope(detached);
            detached.Complete();
            detached.Dispose();

            // has self-detached, and is gone!
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
            Assert.AreSame(context, EfCoreScopeProvider.AmbientScopeContext);
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        Assert.IsNull(EfCoreScopeProvider.AmbientScopeContext);
    }
}
