using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class UnitOfWorkTests : TestWithDatabaseBase
    {
        [Test]
        public void ReadLockNonExisting()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            Assert.Throws<ArgumentException>(() =>
            {
                using (var scope = (Scope)provider.CreateScope())
                {
                    scope.EagerReadLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void ReadLockExisting()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = (Scope)provider.CreateScope())
            {
                scope.EagerReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void WriteLockNonExisting()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            Assert.Throws<ArgumentException>(() =>
            {
                using (var scope = (Scope)provider.CreateScope())
                {
                    scope.EagerWriteLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void WriteLockExisting()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = (Scope)provider.CreateScope())
            {
                scope.EagerWriteLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }
    }
}
