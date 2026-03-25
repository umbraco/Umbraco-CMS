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
    private IEFCoreScopeProvider<TestUmbracoDbContext> EFCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    private EFCoreScopeAccessor<TestUmbracoDbContext> EFCoreScopeAccessor => (EFCoreScopeAccessor<TestUmbracoDbContext>)GetRequiredService<IEFCoreScopeAccessor<TestUmbracoDbContext>>();

    [Test]
    public void CanCreateScope()
    {
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
            Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
    }

    [Test]
    public void CanCreateScopeTwice() =>
        Assert.DoesNotThrow(() =>
        {
            using (var scope = EFCoreScopeProvider.CreateScope())
            {
                scope.Complete();
            }

            using (var scopeTwo = EFCoreScopeProvider.CreateScope())
            {
                scopeTwo.Complete();
            }
        });

    [Test]
    public void NestedCreateScope()
    {
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
            Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);
            using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(nested);
                Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
                Assert.AreSame(nested, EFCoreScopeAccessor.AmbientScope);
                Assert.AreSame(scope, ((EFCoreScope<TestUmbracoDbContext>)nested).ParentScope);
            }
        }

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
    }

    [Test]
    public async Task NestedCreateScopeInnerException()
    {
        bool scopeCompleted = false;

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        try
        {
            using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
            {
                // scopeProvider.Context.Enlist("test", completed => scopeCompleted = completed);
                await scope.ExecuteWithContextAsync(database =>
                {
                    scope.ScopeContext!.Enlist("test", completed => scopeCompleted = completed);
                    Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
                    Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
                    Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);
                    using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
                    {
                        Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(nested);
                        Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
                        Assert.AreSame(nested, EFCoreScopeAccessor.AmbientScope);
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

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        Assert.IsFalse(scopeCompleted);
    }

    [Test]
    public async Task CanAccessDbContext()
    {
        using var scope = EFCoreScopeProvider.CreateScope();
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
        using (var scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                Assert.IsTrue(await database.Database.CanConnectAsync());
                Assert.IsNotNull(database.Database.CurrentTransaction); // in a transaction
            });
            scope.Complete();
        }

        using (var scopeTwo = EFCoreScopeProvider.CreateScope())
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
        using (var scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                Assert.IsTrue(await database.Database.CanConnectAsync());
                var parentTransaction = database.Database.CurrentTransaction;

                using (var nestedSCope = EFCoreScopeProvider.CreateScope())
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
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        IEFCoreScope<TestUmbracoDbContext> mainScope = EFCoreScopeProvider.CreateScope();

        var t = Task.Run(() =>
        {
            IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope();
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
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        IEFCoreScope<TestUmbracoDbContext> mainScope = EFCoreScopeProvider.CreateScope();

        IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope(); // not disposing

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
        Console.WriteLine(ex);
    }

    [Test]
    public void GivenChildThread_WhenParentDisposedBeforeChild_ParentScopeThrows()
    {
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        IEFCoreScope<TestUmbracoDbContext> mainScope = EFCoreScopeProvider.CreateScope();

        var t = Task.Run(() =>
        {
            Console.WriteLine("Child Task start: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);

            // This will push the child scope to the top of the Stack
            IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope();
            Console.WriteLine("Child Task scope created: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
            Thread.Sleep(5000); // block for a bit to ensure the parent task is disposed first
            Console.WriteLine("Child Task before dispose: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
            nested.Dispose();
            Console.WriteLine("Child Task after dispose: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
        });

        // provide some time for the child thread to start so the ambient context is copied in AsyncLocal
        Thread.Sleep(2000);

        // now dispose the main without waiting for the child thread to join
        Console.WriteLine("Parent Task disposing: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);

        // This will throw because at this stage a child scope has been created which means
        // it is the Ambient (top) scope but here we're trying to dispose the non top scope.
        Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
        t.Wait(); // wait for the child to dispose
        mainScope.Dispose(); // now it's ok
        Console.WriteLine("Parent Task disposed: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
    }

    [Test]
    public void GivenChildThread_WhenChildDisposedBeforeParent_OK()
    {
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        IEFCoreScope<TestUmbracoDbContext> mainScope = EFCoreScopeProvider.CreateScope();

        // Task.Run will flow the execution context unless ExecutionContext.SuppressFlow() is explicitly called.
        // This is what occurs in normal async behavior since it is expected to await (and join) the main thread,
        // but if Task.Run is used as a fire and forget thread without being done correctly then the Scope will
        // flow to that thread.
        var t = Task.Run(() =>
        {
            Console.WriteLine("Child Task start: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
            IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope();
            Console.WriteLine("Child Task before dispose: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
            nested.Dispose();
            Console.WriteLine("Child Task after disposed: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
        });

        Console.WriteLine("Parent Task waiting: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
        t.Wait();
        Console.WriteLine("Parent Task disposing: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);
        mainScope.Dispose();
        Console.WriteLine("Parent Task disposed: " + EFCoreScopeAccessor.AmbientScope?.InstanceId);

        Assert.Pass();
    }

    [Test]
    public async Task Transaction()
    {
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(n);
            });
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
            });

            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
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
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp1 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            string n;
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp1 (id, name) VALUES (1, 'a')");
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
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

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
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
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp2 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp2 (id, name) VALUES (1, 'a')");
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
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

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
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
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"INSERT INTO tmp (id, name) VALUES (1, 'a')");
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);

                using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
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

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
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
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);

            // Run on another thread without a flowed context
            Task t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.IsNull(EFCoreScopeAccessor.AmbientScope);

                using (IEFCoreScope<TestUmbracoDbContext> newScope = EFCoreScopeProvider.CreateScope())
                {
                    Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
                    Assert.IsNull(EFCoreScopeAccessor.AmbientScope.ParentScope);
                }

                Assert.IsNull(EFCoreScopeAccessor.AmbientScope);

                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
    }

    [Test]
    public void CallContextScope2()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);

            // Run on another thread without a flowed context
            Task t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.IsNull(EFCoreScopeAccessor.AmbientScope);

                using (IEFCoreScope<TestUmbracoDbContext> newScope = EFCoreScopeProvider.CreateScope())
                {
                    Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
                    Assert.IsNull(EFCoreScopeAccessor.AmbientScope.ParentScope);
                }

                Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ScopeContextEnlist(bool complete)
    {
        bool? completed = null;
        IEFCoreScope<TestUmbracoDbContext> ambientScope = null;
        IScopeContext ambientContext = null;

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            scope.ScopeContext.Enlist("name", c =>
            {
                completed = c;
                ambientScope = EFCoreScopeAccessor.AmbientScope;
                ambientContext = EFCoreScopeProvider.AmbientScopeContext;
            });
            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        Assert.IsNull(EFCoreScopeProvider.AmbientScopeContext);
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

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            scope.ScopeContext.Enlist("name", c =>
            {
                completed = c;

                // at that point the scope is gone, but the context is still there
                IScopeContext ambientContext = EFCoreScopeProvider.AmbientScopeContext;
                ambientContext.Enlist("another", c2 => completed2 = c2);
            });
            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        Assert.IsNull(EFCoreScopeProvider.AmbientScopeContext);
        Assert.IsNotNull(completed);
        Assert.AreEqual(complete, completed.Value);
        Assert.AreEqual(complete, completed2.Value);
    }

    [Test]
    public void DetachableScope()
    {
        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
            Assert.IsNotNull(EFCoreScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);

            Assert.IsNotNull(EFCoreScopeProvider.AmbientScopeContext); // the ambient context
            Assert.IsNotNull(scope.ScopeContext); // the ambient context too (getter only)
            IScopeContext context = scope.ScopeContext;

            var detached = EFCoreScopeProvider.CreateDetachedScope();
            EFCoreScopeProvider.AttachScope(detached);

            Assert.AreEqual(detached, EFCoreScopeAccessor.AmbientScope);
            Assert.AreNotSame(context, EFCoreScopeProvider.AmbientScopeContext);

            // nesting under detached!
            using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
            {
                Assert.Throws<InvalidOperationException>(() =>

                    // cannot detach a non-detachable scope
                    EFCoreScopeProvider.DetachScope());
                nested.Complete();
            }

            Assert.AreEqual(detached, EFCoreScopeAccessor.AmbientScope);
            Assert.AreNotSame(context, EFCoreScopeProvider.AmbientScopeContext);

            // can detach
            Assert.AreSame(detached, EFCoreScopeProvider.DetachScope());

            Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);
            Assert.AreSame(context, EFCoreScopeProvider.AmbientScopeContext);

            Assert.Throws<InvalidOperationException>(() =>

                // cannot disposed a non-attached scope
                // in fact, only the ambient scope can be disposed
                detached.Dispose());

            EFCoreScopeProvider.AttachScope(detached);
            detached.Complete();
            detached.Dispose();

            // has self-detached, and is gone!
            Assert.AreSame(scope, EFCoreScopeAccessor.AmbientScope);
            Assert.AreSame(context, EFCoreScopeProvider.AmbientScopeContext);
        }

        Assert.IsNull(EFCoreScopeAccessor.AmbientScope);
        Assert.IsNull(EFCoreScopeProvider.AmbientScopeContext);
    }
}
