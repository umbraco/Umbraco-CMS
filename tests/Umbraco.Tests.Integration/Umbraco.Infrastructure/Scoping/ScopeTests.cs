// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerFixture)]
    public class ScopeTests : UmbracoIntegrationTest
    {
        private new ScopeProvider ScopeProvider => (ScopeProvider)base.ScopeProvider;

        [SetUp]
        public void SetUp() => Assert.IsNull(ScopeProvider.AmbientScope); // gone

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            // Need to have a mockable request cache for tests
            var appCaches = new AppCaches(
                NoAppCache.Instance,
                Mock.Of<IRequestCache>(x => x.IsAvailable == false),
                new IsolatedCaches(_ => NoAppCache.Instance));

            services.AddUnique(appCaches);
        }

        [Test]
        public void GivenUncompletedScopeOnChildThread_WhenTheParentCompletes_TheTransactionIsRolledBack()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(ScopeProvider.AmbientScope);
            IScope mainScope = scopeProvider.CreateScope();

            var t = Task.Run(() =>
            {
                IScope nested = scopeProvider.CreateScope();
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
            // this all runs in the same execution context so the AmbientScope reference isn't a copy
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(ScopeProvider.AmbientScope);
            IScope mainScope = scopeProvider.CreateScope();

            IScope nested = scopeProvider.CreateScope(); // not disposing

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
            Console.WriteLine(ex);
        }

        [Test]
        public void GivenChildThread_WhenParentDisposedBeforeChild_ParentScopeThrows()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(ScopeProvider.AmbientScope);
            IScope mainScope = scopeProvider.CreateScope();

            var t = Task.Run(() =>
            {
                Console.WriteLine("Child Task start: " + scopeProvider.AmbientScope.InstanceId);

                // This will push the child scope to the top of the Stack
                IScope nested = scopeProvider.CreateScope();
                Console.WriteLine("Child Task scope created: " + scopeProvider.AmbientScope.InstanceId);
                Thread.Sleep(5000); // block for a bit to ensure the parent task is disposed first
                Console.WriteLine("Child Task before dispose: " + scopeProvider.AmbientScope.InstanceId);
                nested.Dispose();
                Console.WriteLine("Child Task after dispose: " + scopeProvider.AmbientScope.InstanceId);
            });

            // provide some time for the child thread to start so the ambient context is copied in AsyncLocal
            Thread.Sleep(2000);

            // now dispose the main without waiting for the child thread to join
            Console.WriteLine("Parent Task disposing: " + scopeProvider.AmbientScope.InstanceId);

            // This will throw because at this stage a child scope has been created which means
            // it is the Ambient (top) scope but here we're trying to dispose the non top scope.
            Assert.Throws<InvalidOperationException>(() => mainScope.Dispose());
            Task.WaitAll(t);        // wait for the child to dispose
            mainScope.Dispose();    // now it's ok
            Console.WriteLine("Parent Task disposed: " + scopeProvider.AmbientScope?.InstanceId);
        }

        [Test]
        public void GivenChildThread_WhenChildDisposedBeforeParent_OK()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(ScopeProvider.AmbientScope);
            IScope mainScope = scopeProvider.CreateScope();

            // Task.Run will flow the execution context unless ExecutionContext.SuppressFlow() is explicitly called.
            // This is what occurs in normal async behavior since it is expected to await (and join) the main thread,
            // but if Task.Run is used as a fire and forget thread without being done correctly then the Scope will
            // flow to that thread.
            var t = Task.Run(() =>
            {
                Console.WriteLine("Child Task start: " + scopeProvider.AmbientScope.InstanceId);
                IScope nested = scopeProvider.CreateScope();
                Console.WriteLine("Child Task before dispose: " + scopeProvider.AmbientScope.InstanceId);
                nested.Dispose();
                Console.WriteLine("Child Task after disposed: " + scopeProvider.AmbientScope.InstanceId);
            });

            Console.WriteLine("Parent Task waiting: " + scopeProvider.AmbientScope?.InstanceId);
            Task.WaitAll(t);
            Console.WriteLine("Parent Task disposing: " + scopeProvider.AmbientScope.InstanceId);
            mainScope.Dispose();
            Console.WriteLine("Parent Task disposed: " + scopeProvider.AmbientScope?.InstanceId);

            Assert.Pass();
        }

        [Test]
        public void SimpleCreateScope()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(ScopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
            }

            Assert.IsNull(scopeProvider.AmbientScope);
        }

        [Test]
        public void SimpleCreateScopeContext()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                Assert.IsNotNull(scopeProvider.AmbientContext);
                Assert.IsNotNull(scopeProvider.Context);
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
        }

        [Test]
        public void SimpleCreateScopeDatabase()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            IUmbracoDatabase database;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                database = ScopeAccessor.AmbientScope.Database; // populates scope's database
                Assert.IsNotNull(database);
                Assert.IsNotNull(database.Connection); // in a transaction
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(database.Connection); // poof gone
        }

        [Test]
        public void NestedCreateScope()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                using (IScope nested = scopeProvider.CreateScope())
                {
                    Assert.IsInstanceOf<Scope>(nested);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(nested, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, ((Scope)nested).ParentScope);
                }
            }

            Assert.IsNull(scopeProvider.AmbientScope);
        }

        [Test]
        public void NestedCreateScopeContext()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                Assert.IsNotNull(scopeProvider.AmbientContext);

                IScopeContext context;
                using (IScope nested = scopeProvider.CreateScope())
                {
                    Assert.IsInstanceOf<Scope>(nested);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(nested, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, ((Scope)nested).ParentScope);

                    Assert.IsNotNull(scopeProvider.Context);
                    Assert.IsNotNull(scopeProvider.AmbientContext);
                    context = scopeProvider.Context;
                }

                Assert.IsNotNull(scopeProvider.AmbientContext);
                Assert.AreSame(context, scopeProvider.AmbientContext);
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
        }

        [Test]
        public void NestedCreateScopeInnerException()
        {
            ScopeProvider scopeProvider = ScopeProvider;
            bool? scopeCompleted = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            try
            {
                using (IScope scope = scopeProvider.CreateScope())
                {
                    scopeProvider.Context.Enlist("test", completed => scopeCompleted = completed);

                    Assert.IsInstanceOf<Scope>(scope);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(scope, scopeProvider.AmbientScope);
                    using (IScope nested = scopeProvider.CreateScope())
                    {
                        Assert.IsInstanceOf<Scope>(nested);
                        Assert.IsNotNull(scopeProvider.AmbientScope);
                        Assert.AreSame(nested, scopeProvider.AmbientScope);
                        Assert.AreSame(scope, ((Scope)nested).ParentScope);
                        nested.Complete();
                        throw new Exception("bang!");
                    }

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

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNotNull(scopeCompleted);
            Assert.IsFalse(scopeCompleted.Value);
        }

        [Test]
        public void NestedCreateScopeDatabase()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            IUmbracoDatabase database;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                database = ScopeAccessor.AmbientScope.Database; // populates scope's database
                Assert.IsNotNull(database);
                Assert.IsNotNull(database.Connection); // in a transaction
                using (IScope nested = scopeProvider.CreateScope())
                {
                    Assert.IsInstanceOf<Scope>(nested);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(nested, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, ((Scope)nested).ParentScope);
                    Assert.AreSame(database, ScopeAccessor.AmbientScope.Database);
                }

                Assert.IsNotNull(database.Connection); // still
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(database.Connection); // poof gone
        }

        [Test]
        public void Transaction()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(n);
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }
        }

        [Test]
        public void NestedTransactionInnerFail()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute($"CREATE TABLE tmp1 (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp1 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IScope nested = scopeProvider.CreateScope())
                {
                    ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp1 (id, name) VALUES (2, 'b')");
                    string nn = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=2");
                    Assert.AreEqual("b", nn);
                }

                n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.AreEqual("b", n);

                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.IsNull(n);
                n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.IsNull(n);
            }
        }

        [Test]
        public void NestedTransactionOuterFail()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("CREATE TABLE tmp2 (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp2 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IScope nested = scopeProvider.CreateScope())
                {
                    ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp2 (id, name) VALUES (2, 'b')");
                    string nn = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=2");
                    Assert.AreEqual("b", nn);
                    nested.Complete();
                }

                n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.AreEqual("b", n);
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.IsNull(n);
                n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.IsNull(n);
            }
        }

        [Test]
        public void NestedTransactionComplete()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("CREATE TABLE tmp (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);

                using (IScope nested = scopeProvider.CreateScope())
                {
                    ScopeAccessor.AmbientScope.Database.Execute("INSERT INTO tmp (id, name) VALUES (2, 'b')");
                    string nn = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                    Assert.AreEqual("b", nn);
                    nested.Complete();
                }

                n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);
                n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
            }
        }

        [Test]
        public void CallContextScope1()
        {
            var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
            ScopeProvider scopeProvider = ScopeProvider;
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.IsNotNull(scopeProvider.AmbientContext);

                // Run on another thread without a flowed context
                Task t = taskHelper.ExecuteBackgroundTask(() =>
                {
                    Assert.IsNull(scopeProvider.AmbientScope);
                    Assert.IsNull(scopeProvider.AmbientContext);

                    using (IScope newScope = scopeProvider.CreateScope())
                    {
                        Assert.IsNotNull(scopeProvider.AmbientScope);
                        Assert.IsNull(scopeProvider.AmbientScope.ParentScope);
                        Assert.IsNotNull(scopeProvider.AmbientContext);
                    }

                    Assert.IsNull(scopeProvider.AmbientScope);
                    Assert.IsNull(scopeProvider.AmbientContext);

                    return Task.CompletedTask;
                });

                Task.WaitAll(t);

                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
        }

        [Test]
        public void CallContextScope2()
        {
            var taskHelper = new TaskHelper(Mock.Of<ILogger<TaskHelper>>());
            ScopeProvider scopeProvider = ScopeProvider;
            Assert.IsNull(scopeProvider.AmbientScope);

            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.IsNotNull(scopeProvider.AmbientContext);

                // Run on another thread without a flowed context
                Task t = taskHelper.ExecuteBackgroundTask(() =>
                {
                    Assert.IsNull(scopeProvider.AmbientScope);
                    Assert.IsNull(scopeProvider.AmbientContext);

                    using (IScope newScope = scopeProvider.CreateScope())
                    {
                        Assert.IsNotNull(scopeProvider.AmbientScope);
                        Assert.IsNull(scopeProvider.AmbientScope.ParentScope);
                        Assert.IsNotNull(scopeProvider.AmbientContext);
                    }

                    Assert.IsNull(scopeProvider.AmbientScope);
                    Assert.IsNull(scopeProvider.AmbientContext);
                    return Task.CompletedTask;
                });

                Task.WaitAll(t);

                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
        }

        [Test]
        public void ScopeReference()
        {
            ScopeProvider scopeProvider = ScopeProvider;
            Scope scope = (Scope) scopeProvider.CreateScope();
            Scope nested = (Scope) scopeProvider.CreateScope();

            Assert.IsNotNull(scopeProvider.AmbientScope);

            var scopeRef = new HttpScopeReference(scopeProvider);
            scopeRef.Register();
            scopeRef.Dispose();

            Assert.IsNull(scopeProvider.AmbientScope);

            Assert.Throws<ObjectDisposedException>(() =>
            {
                IUmbracoDatabase db = scope.Database;
            });
            Assert.Throws<ObjectDisposedException>(() =>
            {
                IUmbracoDatabase db = nested.Database;
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ScopeContextEnlist(bool complete)
        {
            ScopeProvider scopeProvider = ScopeProvider;

            bool? completed = null;
            IScope ambientScope = null;
            IScopeContext ambientContext = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                scopeProvider.Context.Enlist("name", c =>
                {
                    completed = c;
                    ambientScope = scopeProvider.AmbientScope;
                    ambientContext = scopeProvider.AmbientContext;
                });
                if (complete)
                {
                    scope.Complete();
                }
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
            Assert.IsNotNull(completed);
            Assert.AreEqual(complete, completed.Value);
            Assert.IsNull(ambientScope); // the scope is gone
            Assert.IsNotNull(ambientContext); // the context is still there
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ScopeContextEnlistAgain(bool complete)
        {
            ScopeProvider scopeProvider = ScopeProvider;

            bool? completed = null;
            bool? completed2 = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                scopeProvider.Context.Enlist("name", c =>
                {
                    completed = c;

                    // at that point the scope is gone, but the context is still there
                    IScopeContext ambientContext = scopeProvider.AmbientContext;
                    ambientContext.Enlist("another", c2 => completed2 = c2);
                });
                if (complete)
                {
                    scope.Complete();
                }
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
            Assert.IsNotNull(completed);
            Assert.AreEqual(complete, completed.Value);
            Assert.AreEqual(complete, completed2.Value);
        }

        [Test]
        public void ScopeContextException()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            bool? completed = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                IScope detached = scopeProvider.CreateDetachedScope();
                scopeProvider.AttachScope(detached);

                // the exception does not prevent other enlisted items to run
                // *and* it does not prevent the scope from properly going down
                scopeProvider.Context.Enlist("name", c => throw new Exception("bang"));
                scopeProvider.Context.Enlist("other", c => completed = c);
                detached.Complete();
                Assert.Throws<AggregateException>(() => detached.Dispose());

                // even though disposing of the scope has thrown, it has exited
                // properly ie it has removed itself, and the app remains clean
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                scope.Complete();
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);

            Assert.IsNotNull(completed);
            Assert.AreEqual(true, completed);
        }

        [Test]
        public void DetachableScope()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                Assert.IsNotNull(scopeProvider.AmbientContext); // the ambient context
                Assert.IsNotNull(scopeProvider.Context); // the ambient context too (getter only)
                IScopeContext context = scopeProvider.Context;

                IScope detached = scopeProvider.CreateDetachedScope();
                scopeProvider.AttachScope(detached);

                Assert.AreEqual(detached, scopeProvider.AmbientScope);
                Assert.AreNotSame(context, scopeProvider.Context);

                // nesting under detached!
                using (IScope nested = scopeProvider.CreateScope())
                {
                    Assert.Throws<InvalidOperationException>(() =>

                        // cannot detach a non-detachable scope
                        scopeProvider.DetachScope());
                    nested.Complete();
                }

                Assert.AreEqual(detached, scopeProvider.AmbientScope);
                Assert.AreNotSame(context, scopeProvider.Context);

                // can detach
                Assert.AreSame(detached, scopeProvider.DetachScope());

                Assert.AreSame(scope, scopeProvider.AmbientScope);
                Assert.AreSame(context, scopeProvider.AmbientContext);

                Assert.Throws<InvalidOperationException>(() =>

                    // cannot disposed a non-attached scope
                    // in fact, only the ambient scope can be disposed
                    detached.Dispose());

                scopeProvider.AttachScope(detached);
                detached.Complete();
                detached.Dispose();

                // has self-detached, and is gone!
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                Assert.AreSame(context, scopeProvider.AmbientContext);
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
        }
    }
}
