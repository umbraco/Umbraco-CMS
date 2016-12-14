using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class DatabaseScopeTests
    {
        [TearDown]
        public void TearDown()
        {
            SafeCallContext.Clear();
        }

        [Test]
        public void CreateAndDispose()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope = new DatabaseScope(accessor, null);
            Assert.AreSame(scope, accessor.Scope);
            scope.Dispose();
            Assert.IsNull(accessor.Scope);
        }

        [Test]
        public void DisposeDisposed()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope = new DatabaseScope(accessor, null);
            scope.Dispose();
            Assert.Throws<ObjectDisposedException>(() => scope.Dispose());
        }

        [Test]
        public void DisposeParent()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope1 = new DatabaseScope(accessor, null);
            var scope2 = new DatabaseScope(accessor, null);
            Assert.Throws<InvalidOperationException>(() => scope1.Dispose());
            scope2.Dispose();
            scope1.Dispose();
        }

        [Test]
        public void DatabaseDisposed()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope = new DatabaseScope(accessor, null);
            scope.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
            {
                var database = scope.Database;
            });
        }

        [Test]
        public void NestScopes()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope1 = new DatabaseScope(accessor, null);
            var scope2 = new DatabaseScope(accessor, null);
            Assert.AreSame(scope2, accessor.Scope);
            scope2.Dispose();
            Assert.AreSame(scope1, accessor.Scope);
            scope1.Dispose();
            Assert.IsNull(accessor.Scope);
        }

        [Test]
        public void NestedScopeSameDatabase()
        {
            var logger = Mock.Of<ILogger>();
            var providers = new TestObjects(null).GetDefaultSqlSyntaxProviders(logger);
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var factory = new UmbracoDatabaseFactory(providers, logger, accessor, new MapperCollection(Enumerable.Empty<BaseMapper>()));
            var scope1 = new DatabaseScope(accessor, factory);
            var database1 = accessor.Scope.Database;
            Assert.IsNotNull(database1);
            var scope2 = new DatabaseScope(accessor, factory);
            var database2 = accessor.Scope.Database;
            Assert.AreSame(database1, database2);
            scope2.Dispose();
            scope1.Dispose();
        }

        [Test]
        public void NestedScopeForceDatabase()
        {
            var logger = Mock.Of<ILogger>();
            var providers = new TestObjects(null).GetDefaultSqlSyntaxProviders(logger);
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var factory = new UmbracoDatabaseFactory(providers, logger, accessor, new MapperCollection(Enumerable.Empty<BaseMapper>()));
            var scope1 = new DatabaseScope(accessor, factory);
            var database1 = accessor.Scope.Database;
            Assert.IsNotNull(database1);
            var database2 = factory.CreateDatabase();
            Assert.AreNotSame(database1, database2);
            var scope2 = new DatabaseScope(accessor, factory, database2);
            Assert.AreSame(database2, accessor.Scope.Database);
            scope2.Dispose();
            Assert.IsNotNull(accessor.Scope);
            Assert.AreSame(database1, accessor.Scope.Database);
            scope1.Dispose();
            Assert.IsNull(accessor.Scope);
        }

        [Test]
        public void FlowsWithAsync()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope = new DatabaseScope(accessor, null);
            var ascope = GetScopeAsync(accessor).Result;
            Assert.AreSame(scope, ascope);
        }

        [Test]
        public void FlowsWithThread()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope = new DatabaseScope(accessor, null);
            DatabaseScope tscope = null;
            var thread = new Thread(() => tscope = accessor.Scope);
            thread.Start();
            thread.Join();
            Assert.AreSame(scope, tscope);
        }

        [Test]
        public void DoesNotFlowWithAsyncAndSafeCallContext()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope = new DatabaseScope(accessor, null);
            DatabaseScope ascope;
            using (new SafeCallContext())
            {
                ascope = GetScopeAsync(accessor).Result;
            }
            Assert.IsNull(ascope);

            Assert.AreSame(scope, accessor.Scope);
            scope.Dispose();
            Assert.IsNull(accessor.Scope);
        }

        [Test]
        public void DoesNotFlowWithThreadAndSafeCallContext()
        {
            var httpContextAccessor = new NoHttpContextAccessor();
            var accessor = new HybridDatabaseScopeAccessor(httpContextAccessor);
            var scope = new DatabaseScope(accessor, null);
            DatabaseScope tscope = null;
            var thread = new Thread(() => tscope = accessor.Scope);
            using (new SafeCallContext())
            {
                thread.Start();
            }
            thread.Join();
            Assert.IsNull(tscope);

            Assert.AreSame(scope, accessor.Scope);
            scope.Dispose();
            Assert.IsNull(accessor.Scope);
        }

        private static async Task<DatabaseScope> GetScopeAsync(IDatabaseScopeAccessor accessor)
        {
            await Task.Delay(1);
            return accessor.Scope;
        }
    }
}
