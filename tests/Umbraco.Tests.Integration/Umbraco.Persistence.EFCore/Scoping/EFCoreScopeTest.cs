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
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.That(scope, Is.InstanceOf<EFCoreScope<TestUmbracoDbContext>>());
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
        }

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
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
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.That(scope, Is.InstanceOf<EFCoreScope<TestUmbracoDbContext>>());
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
            using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
            {
                Assert.That(nested, Is.InstanceOf<EFCoreScope<TestUmbracoDbContext>>());
                Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
                Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(nested));
                Assert.That(((EFCoreScope<TestUmbracoDbContext>)nested).ParentScope, Is.SameAs(scope));
            }
        }

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
    }

    [Test]
    public async Task NestedCreateScopeInnerException()
    {
        bool scopeCompleted = false;

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        try
        {
            using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
            {
                // scopeProvider.Context.Enlist("test", completed => scopeCompleted = completed);
                await scope.ExecuteWithContextAsync(database =>
                {
                    scope.ScopeContext!.Enlist("test", completed => scopeCompleted = completed);
                    Assert.That(scope, Is.InstanceOf<EFCoreScope<TestUmbracoDbContext>>());
                    Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
                    Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
                    using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
                    {
                        Assert.That(nested, Is.InstanceOf<EFCoreScope<TestUmbracoDbContext>>());
                        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
                        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(nested));
                        Assert.That(((EFCoreScope<TestUmbracoDbContext>)nested).ParentScope, Is.SameAs(scope));
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

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        Assert.That(scopeCompleted, Is.False);
    }

    [Test]
    public async Task CanAccessDbContext()
    {
        using var scope = EFCoreScopeProvider.CreateScope();
        await scope.ExecuteWithContextAsync<Task>(async database =>
        {
            Assert.That(await database.Database.CanConnectAsync(), Is.True);
            Assert.That(database.Database.CurrentTransaction, Is.Not.Null); // in a transaction
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
                Assert.That(await database.Database.CanConnectAsync(), Is.True);
                Assert.That(database.Database.CurrentTransaction, Is.Not.Null); // in a transaction
            });
            scope.Complete();
        }

        using (var scopeTwo = EFCoreScopeProvider.CreateScope())
        {
            await scopeTwo.ExecuteWithContextAsync<Task>(async database =>
            {
                Assert.That(await database.Database.CanConnectAsync(), Is.True);
                Assert.That(database.Database.CurrentTransaction, Is.Not.Null); // in a transaction
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
                Assert.That(await database.Database.CanConnectAsync(), Is.True);
                var parentTransaction = database.Database.CurrentTransaction;

                using (var nestedScope = EFCoreScopeProvider.CreateScope())
                {
                    await nestedScope.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        Assert.That(await nestedDatabase.Database.CanConnectAsync(), Is.True);
                        Assert.That(nestedDatabase.Database.CurrentTransaction, Is.Not.Null); // in a transaction
                        var childTransaction = nestedDatabase.Database.CurrentTransaction;
                        Assert.That(childTransaction, Is.SameAs(parentTransaction));
                    });
                }
            });
            scope.Complete();
        }
    }

    [Test]
    public void GivenUncompletedScopeOnChildThread_WhenTheParentCompletes_TheTransactionIsRolledBack()
    {
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
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
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        IEFCoreScope<TestUmbracoDbContext> mainScope = EFCoreScopeProvider.CreateScope();

        IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope(); // not disposing

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
        Console.WriteLine(ex);
    }

    [Test]
    public void GivenChildThread_WhenParentDisposedBeforeChild_ParentScopeThrows()
    {
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
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
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
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
                Assert.That(result, Is.EqualTo("a"));
            });
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.That(n, Is.Null);
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
                Assert.That(n, Is.EqualTo("a"));
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
                Assert.That(n, Is.EqualTo("a"));

                using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
                {
                    await nested.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        await nestedDatabase.Database.ExecuteSqlAsync($"INSERT INTO tmp1 (id, name) VALUES (2, 'b')");
                        string nn = await nestedDatabase.Database.ExecuteScalarAsync<string>(
                            "SELECT name FROM tmp1 WHERE id=2");
                        Assert.That(nn, Is.EqualTo("b"));
                    });
                }

                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.That(n, Is.EqualTo("b"));
            });

            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.That(n, Is.Null);
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.That(n, Is.Null);
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
                Assert.That(n, Is.EqualTo("a"));

                using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
                {
                    await scope.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        await nestedDatabase.Database.ExecuteSqlAsync($"INSERT INTO tmp2 (id, name) VALUES (2, 'b')");
                        string nn = await nestedDatabase.Database.ExecuteScalarAsync<string>(
                            "SELECT name FROM tmp2 WHERE id=2");
                        Assert.That(nn, Is.EqualTo("b"));
                    });

                    nested.Complete();
                }

                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.That(n, Is.EqualTo("b"));
            });
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.That(n, Is.Null);
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.That(n, Is.Null);
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
                Assert.That(n, Is.EqualTo("a"));

                using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
                {
                    await scope.ExecuteWithContextAsync<Task>(async nestedDatabase =>
                    {
                        await nestedDatabase.Database.ExecuteSqlAsync($"INSERT INTO tmp (id, name) VALUES (2, 'b')");
                        string nn =
                            await nestedDatabase.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=2");
                        Assert.That(nn, Is.EqualTo("b"));
                    });

                    nested.Complete();
                }

                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=2");
                Assert.That(n, Is.EqualTo("b"));
            });

            scope.Complete();
        }

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=1");
                Assert.That(n, Is.EqualTo("a"));
                n = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp WHERE id=2");
                Assert.That(n, Is.EqualTo("b"));
            });
        }
    }

    [Test]
    public void CallContextScope1()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);

            // Run on another thread without a flowed context
            Task t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);

                using (IEFCoreScope<TestUmbracoDbContext> newScope = EFCoreScopeProvider.CreateScope())
                {
                    Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
                    Assert.That(EFCoreScopeAccessor.AmbientScope.ParentScope, Is.Null);
                }

                Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);

                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
        }

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
    }

    [Test]
    public void CallContextScope2()
    {
        var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);

        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);

            // Run on another thread without a flowed context
            Task t = taskHelper.ExecuteBackgroundTask(() =>
            {
                Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);

                using (IEFCoreScope<TestUmbracoDbContext> newScope = EFCoreScopeProvider.CreateScope())
                {
                    Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
                    Assert.That(EFCoreScopeAccessor.AmbientScope.ParentScope, Is.Null);
                }

                Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
                return Task.CompletedTask;
            });

            Task.WaitAll(t);

            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
        }

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ScopeContextEnlist(bool complete)
    {
        bool? completed = null;
        IEFCoreScope<TestUmbracoDbContext> ambientScope = null;
        IScopeContext ambientContext = null;

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
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

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.Null);
        Assert.That(completed, Is.Not.Null);
        Assert.That(completed.Value, Is.EqualTo(complete));
        Assert.That(ambientScope, Is.Null); // the scope is gone
        Assert.That(ambientContext, Is.Not.Null); // the context is still there
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ScopeContextEnlistAgain(bool complete)
    {
        bool? completed = null;
        bool? completed2 = null;

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
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

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.Null);
        Assert.That(completed, Is.Not.Null);
        Assert.That(completed.Value, Is.EqualTo(complete));
        Assert.That(completed2.Value, Is.EqualTo(complete));
    }

    [Test]
    public void DetachableScope()
    {
        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        using (IEFCoreScope<TestUmbracoDbContext> scope = EFCoreScopeProvider.CreateScope())
        {
            Assert.That(scope, Is.InstanceOf<EFCoreScope<TestUmbracoDbContext>>());
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Not.Null);
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));

            Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.Not.Null); // the ambient context
            Assert.That(scope.ScopeContext, Is.Not.Null); // the ambient context too (getter only)
            IScopeContext context = scope.ScopeContext;

            IEFCoreScope<TestUmbracoDbContext> detached = EFCoreScopeProvider.CreateDetachedScope();
            EFCoreScopeProvider.AttachScope(detached);

            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.EqualTo(detached));
            Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.Not.SameAs(context));

            // nesting under detached!
            using (IEFCoreScope<TestUmbracoDbContext> nested = EFCoreScopeProvider.CreateScope())
            {
                Assert.Throws<InvalidOperationException>(() =>

                    // cannot detach a non-detachable scope
                    EFCoreScopeProvider.DetachScope());
                nested.Complete();
            }

            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.EqualTo(detached));
            Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.Not.SameAs(context));

            // can detach
            Assert.That(EFCoreScopeProvider.DetachScope(), Is.SameAs(detached));

            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
            Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.SameAs(context));

            Assert.Throws<InvalidOperationException>(() =>

                // cannot disposed a non-attached scope
                // in fact, only the ambient scope can be disposed
                detached.Dispose());

            EFCoreScopeProvider.AttachScope(detached);
            detached.Complete();
            detached.Dispose();

            // has self-detached, and is gone!
            Assert.That(EFCoreScopeAccessor.AmbientScope, Is.SameAs(scope));
            Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.SameAs(context));
        }

        Assert.That(EFCoreScopeAccessor.AmbientScope, Is.Null);
        Assert.That(EFCoreScopeProvider.AmbientScopeContext, Is.Null);
    }
}
