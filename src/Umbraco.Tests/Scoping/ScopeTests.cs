using System;
using System.Collections;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.EmptyDbFilePerTest)]
    public class ScopeTests : BaseDatabaseFactoryTest
    {
        // setup
        public override void Initialize()
        {
            base.Initialize();

            Assert.IsNull(DatabaseContext.ScopeProvider.AmbientScope); // gone
        }

        [Test]
        public void SimpleCreateScope()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
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
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
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
            var scopeProvider = DatabaseContext.ScopeProvider;

            UmbracoDatabase database;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
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
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                using (var nested = scopeProvider.CreateScope())
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
            var scopeProvider = DatabaseContext.ScopeProvider;
            Assert.IsNull(scopeProvider.AmbientScope);

            var httpContextItems = new Hashtable();
            ScopeProvider.HttpContextItemsGetter = () => httpContextItems;
            try
            {
                using (var scope = scopeProvider.CreateScope())
                {
                    Assert.IsInstanceOf<Scope>(scope);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(scope, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, httpContextItems[ScopeProvider.ScopeItemKey]);

                    // only if Core.DEBUG_SCOPES are defined
                    //Assert.IsEmpty(scopeProvider.CallContextObjects);

                    using (var nested = scopeProvider.CreateScope(callContext: true))
                    {
                        Assert.IsInstanceOf<Scope>(nested);
                        Assert.IsNotNull(scopeProvider.AmbientScope);
                        Assert.AreSame(nested, scopeProvider.AmbientScope);
                        Assert.AreSame(scope, ((Scope) nested).ParentScope);

                        // it's moved over to call context
                        Assert.IsNull(httpContextItems[ScopeProvider.ScopeItemKey]);
                        var callContextKey = CallContext.LogicalGetData(ScopeProvider.ScopeItemKey).AsGuid();
                        Assert.AreNotEqual(Guid.Empty, callContextKey);

                        // only if Core.DEBUG_SCOPES are defined
                        //var ccnested = scopeProvider.CallContextObjects[callContextKey];
                        //Assert.AreSame(nested, ccnested);
                    }

                    // it's naturally back in http context
                    Assert.AreSame(scope, httpContextItems[ScopeProvider.ScopeItemKey]);
                }
                Assert.IsNull(scopeProvider.AmbientScope);
            }
            finally
            {
                ScopeProvider.HttpContextItemsGetter = null;
            }
        }

        [Test]
        public void NestedCreateScopeContext()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                Assert.IsNotNull(scopeProvider.AmbientContext);

                ScopeContext context;
                using (var nested = scopeProvider.CreateScope())
                {
                    Assert.IsInstanceOf<Scope>(nested);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(nested, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, ((Scope) nested).ParentScope);

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
            var scopeProvider = DatabaseContext.ScopeProvider;
            bool? scopeCompleted = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            try
            {
                using (var scope = scopeProvider.CreateScope())
                {
                    scopeProvider.Context.Enlist("test", completed => scopeCompleted = completed);

                    Assert.IsInstanceOf<Scope>(scope);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(scope, scopeProvider.AmbientScope);
                    using (var nested = scopeProvider.CreateScope())
                    {
                        Assert.IsInstanceOf<Scope>(nested);
                        Assert.IsNotNull(scopeProvider.AmbientScope);
                        Assert.AreSame(nested, scopeProvider.AmbientScope);
                        Assert.AreSame(scope, ((Scope) nested).ParentScope);
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
                    Assert.Fail("Wrong exception.");
            }
            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNotNull(scopeCompleted);
            Assert.IsFalse(scopeCompleted.Value);
        }

        [Test]
        public void NestedCreateScopeDatabase()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            UmbracoDatabase database;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                database = scope.Database; // populates scope's database
                Assert.IsNotNull(database);
                Assert.IsNotNull(database.Connection); // in a transaction
                using (var nested = scopeProvider.CreateScope())
                {
                    Assert.IsInstanceOf<Scope>(nested);
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(nested, scopeProvider.AmbientScope);
                    Assert.AreSame(scope, ((Scope) nested).ParentScope);
                    Assert.AreSame(database, nested.Database);
                }
                Assert.IsNotNull(database.Connection); // still
            }
            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(database.Connection); // poof gone
        }

        [Test]
        public void SimpleNoScope()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.GetAmbientOrNoScope())
            {
                Assert.IsInstanceOf<NoScope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
            }
            Assert.IsNull(scopeProvider.AmbientScope);
        }

        [Test]
        public void SimpleNoScopeDatabase()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            UmbracoDatabase database;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.GetAmbientOrNoScope())
            {
                Assert.IsInstanceOf<NoScope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);
                database = scope.Database; // populates scope's database
                Assert.IsNotNull(database);
                Assert.IsNull(database.Connection); // no transaction
            }
            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(database.Connection); // still
        }

        [Test]
        public void NestedNoScope()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            var scope = scopeProvider.GetAmbientOrNoScope();
            Assert.IsInstanceOf<NoScope>(scope);
            Assert.IsNotNull(scopeProvider.AmbientScope);
            Assert.AreSame(scope, scopeProvider.AmbientScope);

            using (var nested = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(nested);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(nested, scopeProvider.AmbientScope);

                // nested does not have a parent
                Assert.IsNull(((Scope) nested).ParentScope);
            }

            // and when nested is gone, scope is gone
            Assert.IsNull(scopeProvider.AmbientScope);
        }

        [Test]
        public void NestedNoScopeDatabase()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            var scope = scopeProvider.GetAmbientOrNoScope();
            Assert.IsInstanceOf<NoScope>(scope);
            Assert.IsNotNull(scopeProvider.AmbientScope);
            Assert.AreSame(scope, scopeProvider.AmbientScope);
            var database = scope.Database;
            Assert.IsNotNull(database);
            Assert.IsNull(database.Connection); // no transaction

            using (var nested = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(nested);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(nested, scopeProvider.AmbientScope);
                var nestedDatabase = nested.Database; // causes transaction
                Assert.AreSame(database, nestedDatabase); // stolen
                Assert.IsNotNull(database.Connection); // no more

                // nested does not have a parent
                Assert.IsNull(((Scope) nested).ParentScope);
            }

            // and when nested is gone, scope is gone
            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(database.Connection); // poof gone
        }

        [Test]
        public void NestedNoScopeFail()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            var scope = scopeProvider.GetAmbientOrNoScope();
            Assert.IsInstanceOf<NoScope>(scope);
            Assert.IsNotNull(scopeProvider.AmbientScope);
            Assert.AreSame(scope, scopeProvider.AmbientScope);
            var database = scope.Database;
            Assert.IsNotNull(database);
            Assert.IsNull(database.Connection); // no transaction
            database.BeginTransaction();
            Assert.IsNotNull(database.Connection); // now there is one

            Assert.Throws<Exception>(() =>
            {
                // could not steal the database
                /*var nested =*/
                scopeProvider.CreateScope();
            });

            // cleanup
            database.CompleteTransaction();
        }

        [Test]
        public void NoScopeNested()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // AmbientOrNoScope returns the ambient scope
                Assert.AreSame(scope, scopeProvider.GetAmbientOrNoScope());
            }
        }

        [Test]
        public void Transaction()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var noScope = scopeProvider.GetAmbientOrNoScope();
            var database = noScope.Database;
            database.Execute("CREATE TABLE tmp (id INT, name NVARCHAR(64))");

            using (var scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp (id, name) VALUES (1, 'a')");
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);
            }

            using (var scope = scopeProvider.CreateScope())
            {
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.IsNull(n);
            }

            using (var scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp (id, name) VALUES (1, 'a')");
                scope.Complete();
            }

            using (var scope = scopeProvider.CreateScope())
            {
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);
            }
        }

        [Test]
        public void NestedTransactionInnerFail()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var noScope = scopeProvider.GetAmbientOrNoScope();
            var database = noScope.Database;
            database.Execute("CREATE TABLE tmp (id INT, name NVARCHAR(64))");

            using (var scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp (id, name) VALUES (1, 'a')");
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);

                using (var nested = scopeProvider.CreateScope())
                {
                    nested.Database.Execute("INSERT INTO tmp (id, name) VALUES (2, 'b')");
                    var nn = nested.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                    Assert.AreEqual("b", nn);
                }

                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);

                scope.Complete();
            }

            using (var scope = scopeProvider.CreateScope())
            {
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.IsNull(n);
                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.IsNull(n);
            }
        }

        [Test]
        public void NestedTransactionOuterFail()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var noScope = scopeProvider.GetAmbientOrNoScope();
            var database = noScope.Database;
            database.Execute("CREATE TABLE tmp (id INT, name NVARCHAR(64))");

            using (var scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp (id, name) VALUES (1, 'a')");
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);

                using (var nested = scopeProvider.CreateScope())
                {
                    nested.Database.Execute("INSERT INTO tmp (id, name) VALUES (2, 'b')");
                    var nn = nested.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                    Assert.AreEqual("b", nn);
                    nested.Complete();
                }

                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
            }

            using (var scope = scopeProvider.CreateScope())
            {
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.IsNull(n);
                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.IsNull(n);
            }
        }

        [Test]
        public void NestedTransactionComplete()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var noScope = scopeProvider.GetAmbientOrNoScope();
            var database = noScope.Database;
            database.Execute("CREATE TABLE tmp (id INT, name NVARCHAR(64))");

            using (var scope = scopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp (id, name) VALUES (1, 'a')");
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);

                using (var nested = scopeProvider.CreateScope())
                {
                    nested.Database.Execute("INSERT INTO tmp (id, name) VALUES (2, 'b')");
                    var nn = nested.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                    Assert.AreEqual("b", nn);
                    nested.Complete();
                }

                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
                scope.Complete();
            }

            using (var scope = scopeProvider.CreateScope())
            {
                var n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=1");
                Assert.AreEqual("a", n);
                n = scope.Database.ExecuteScalar<string>("SELECT name FROM tmp WHERE id=2");
                Assert.AreEqual("b", n);
            }
        }

        [Test]
        public void CallContextScope1()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.IsNotNull(scopeProvider.AmbientContext);
                using (new SafeCallContext())
                {
                    Assert.IsNull(scopeProvider.AmbientScope);
                    Assert.IsNull(scopeProvider.AmbientContext);

                    using (var newScope = scopeProvider.CreateScope())
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
            var scopeProvider = DatabaseContext.ScopeProvider;
            Assert.IsNull(scopeProvider.AmbientScope);

            var httpContextItems = new Hashtable();
            ScopeProvider.HttpContextItemsGetter = () => httpContextItems;
            try
            {
                using (var scope = scopeProvider.CreateScope())
                {
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.IsNotNull(scopeProvider.AmbientContext);
                    using (new SafeCallContext())
                    {
                        // pretend it's another thread
                        ScopeProvider.HttpContextItemsGetter = null;

                        Assert.IsNull(scopeProvider.AmbientScope);
                        Assert.IsNull(scopeProvider.AmbientContext);

                        using (var newScope = scopeProvider.CreateScope())
                        {
                            Assert.IsNotNull(scopeProvider.AmbientScope);
                            Assert.IsNull(scopeProvider.AmbientScope.ParentScope);
                            Assert.IsNotNull(scopeProvider.AmbientContext);
                        }

                        Assert.IsNull(scopeProvider.AmbientScope);
                        Assert.IsNull(scopeProvider.AmbientContext);

                        // back to original thread
                        ScopeProvider.HttpContextItemsGetter = () => httpContextItems;
                    }
                    Assert.IsNotNull(scopeProvider.AmbientScope);
                    Assert.AreSame(scope, scopeProvider.AmbientScope);
                }

                Assert.IsNull(scopeProvider.AmbientScope);
                Assert.IsNull(scopeProvider.AmbientContext);
            }
            finally
            {
                ScopeProvider.HttpContextItemsGetter = null;
            }
        }

        [Test]
        public void ScopeReference()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var scope = scopeProvider.CreateScope();
            var nested = scopeProvider.CreateScope();
            Assert.IsNotNull(scopeProvider.AmbientScope);
            var scopeRef = new ScopeReference(scopeProvider);
            scopeRef.Dispose();
            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var db = scope.Database;
            });
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var db = nested.Database;
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ScopeContextEnlist(bool complete)
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            bool? completed = null;
            IScope ambientScope = null;
            ScopeContext ambientContext = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                scopeProvider.Context.Enlist("name", c =>
                {
                    completed = c;
                    ambientScope = scopeProvider.AmbientScope;
                    ambientContext = scopeProvider.AmbientContext;
                });
                if (complete)
                    scope.Complete();
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
            var scopeProvider = DatabaseContext.ScopeProvider;

            bool? completed = null;
            Exception exception = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                scopeProvider.Context.Enlist("name", c =>
                {
                    completed = c;

                    // at that point the scope is gone, but the context is still there
                    var ambientContext = scopeProvider.AmbientContext;

                    try
                    {
                        ambientContext.Enlist("another", c2 => { });
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                });
                if (complete)
                    scope.Complete();
            }
            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
            Assert.IsNotNull(completed);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<InvalidOperationException>(exception);
        }

        [Test]
        public void ScopeContextException()
        {
            var scopeProvider = DatabaseContext.ScopeProvider;

            bool? completed = null;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                var detached = scopeProvider.CreateDetachedScope();
                scopeProvider.AttachScope(detached);
                // the exception does not prevent other enlisted items to run
                // *and* it does not prevent the scope from properly going down
                scopeProvider.Context.Enlist("name", c =>
                {
                    throw new Exception("bang");
                });
                scopeProvider.Context.Enlist("other", c =>
                {
                    completed = c;
                });
                detached.Complete();
                Assert.Throws<AggregateException>(() =>
                {
                    detached.Dispose();
                });

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
            var scopeProvider = DatabaseContext.ScopeProvider;

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                Assert.IsNotNull(scopeProvider.AmbientContext); // the ambient context
                Assert.IsNotNull(scopeProvider.Context); // the ambient context too (getter only)
                var context = scopeProvider.Context;

                var detached = scopeProvider.CreateDetachedScope();
                scopeProvider.AttachScope(detached);

                Assert.AreEqual(detached, scopeProvider.AmbientScope);
                Assert.AreNotSame(context, scopeProvider.Context);

                // nesting under detached!
                using (var nested = scopeProvider.CreateScope())
                {
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        // cannot detach a non-detachable scope
                        scopeProvider.DetachScope();
                    });
                    nested.Complete();
                }

                Assert.AreEqual(detached, scopeProvider.AmbientScope);
                Assert.AreNotSame(context, scopeProvider.Context);

                // can detach
                Assert.AreSame(detached, scopeProvider.DetachScope());

                Assert.AreSame(scope, scopeProvider.AmbientScope);
                Assert.AreSame(context, scopeProvider.AmbientContext);

                Assert.Throws<InvalidOperationException>(() =>
                {
                    // cannot disposed a non-attached scope
                    // in fact, only the ambient scope can be disposed
                    detached.Dispose();
                });

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