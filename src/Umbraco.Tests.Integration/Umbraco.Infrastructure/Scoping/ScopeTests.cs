// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerFixture)]
    public class ScopeTests : UmbracoIntegrationTest
    {
        private new ScopeProvider ScopeProvider => (ScopeProvider)base.ScopeProvider;

        [SetUp]
        public void SetUp() => Assert.IsNull(ScopeProvider.AmbientScope); // gone

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
                database = scope.Database; // populates scope's database
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
        public void NestedMigrateScope()
        {
            ScopeProvider scopeProvider = ScopeProvider;
            Assert.IsNull(scopeProvider.AmbientScope);

            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // only if Core.DEBUG_SCOPES are defined
                //// Assert.IsEmpty(scopeProvider.CallContextObjects);

                using (IScope nested = scopeProvider.CreateScope(callContext: true))
                {
                    Assert.IsInstanceOf<Scope>(nested);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(nested, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, ((Scope)nested).ParentScope);

                    // it's moved over to call context
                    IScope callContextScope = CallContext<IScope>.GetData(ScopeProvider.ScopeItemKey);
                    Assert.IsNotNull(callContextScope);

                    // only if Core.DEBUG_SCOPES are defined
                    // var ccnested = scopeProvider.CallContextObjects[callContextKey];
                    // Assert.AreSame(nested, ccnested);
                }

                // it's naturally back in http context
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
                database = scope.Database; // populates scope's database
                Assert.IsNotNull(database);
                Assert.IsNotNull(database.Connection); // in a transaction
                using (IScope nested = scopeProvider.CreateScope())
                {
                    Assert.IsInstanceOf<Scope>(nested);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(nested, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, ((Scope)nested).ParentScope);
                    Assert.AreSame(database, nested.Database);
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
                scope.Database.Execute("CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(n);
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }
        }

        [Test]
        public void NestedTransactionInnerFail()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute($"CREATE TABLE tmp1 (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp1 (id, name) VALUES (1, 'a')");
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IScope nested = scopeProvider.CreateScope())
                {
                    nested.Database.Execute("INSERT INTO tmp1 (id, name) VALUES (2, 'b')");
                    string nn = nested.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=2");
                    Assert.AreEqual("b", nn);
                }

                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.AreEqual("b", n);

                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=1");
                Assert.IsNull(n);
                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp1 WHERE id=2");
                Assert.IsNull(n);
            }
        }

        [Test]
        public void NestedTransactionOuterFail()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("CREATE TABLE tmp2 (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp2 (id, name) VALUES (1, 'a')");
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.AreEqual("a", n);

                using (IScope nested = scopeProvider.CreateScope())
                {
                    nested.Database.Execute("INSERT INTO tmp2 (id, name) VALUES (2, 'b')");
                    string nn = nested.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=2");
                    Assert.AreEqual("b", nn);
                    nested.Complete();
                }

                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.AreEqual("b", n);
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=1");
                Assert.IsNull(n);
                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp2 WHERE id=2");
                Assert.IsNull(n);
            }
        }

        [Test]
        public void NestedTransactionComplete()
        {
            ScopeProvider scopeProvider = ScopeProvider;

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("CREATE TABLE tmp (id INT, name NVARCHAR(64))");
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp (id, name) VALUES (1, 'a')");
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);

                using (IScope nested = scopeProvider.CreateScope())
                {
                    nested.Database.Execute("INSERT INTO tmp (id, name) VALUES (2, 'b')");
                    string nn = nested.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                    Assert.AreEqual("b", nn);
                    nested.Complete();
                }

                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
                scope.Complete();
            }

            using (IScope scope = scopeProvider.CreateScope())
            {
                string n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);
                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
            }
        }

        [Test]
        public void CallContextScope1()
        {
            ScopeProvider scopeProvider = ScopeProvider;
            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.IsNotNull(scopeProvider.AmbientContext);
                using (new SafeCallContext())
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
                }

                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
            }

            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
        }

        [Test]
        public void CallContextScope2()
        {
            ScopeProvider scopeProvider = ScopeProvider;
            Assert.IsNull(scopeProvider.AmbientScope);

            using (IScope scope = scopeProvider.CreateScope())
            {
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.IsNotNull(scopeProvider.AmbientContext);
                using (new SafeCallContext())
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
                }

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
            IScope scope = scopeProvider.CreateScope();
            IScope nested = scopeProvider.CreateScope();
            Assert.IsNotNull(scopeProvider.AmbientScope);
            var scopeRef = new ScopeReference(scopeProvider);
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
