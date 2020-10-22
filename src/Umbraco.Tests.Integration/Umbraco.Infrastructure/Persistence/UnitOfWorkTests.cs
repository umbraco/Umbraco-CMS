using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class UnitOfWorkTests : UmbracoIntegrationTest
    {
        [Test]
        public void ReadLockNonExisting()
        {
            var provider = ScopeProvider;
            Assert.Throws<ArgumentException>(() =>
            {
                using (var scope = provider.CreateScope())
                {
                    scope.ReadLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void ReadLockExisting()
        {
            var provider = ScopeProvider;
            using (var scope = provider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void WriteLockNonExisting()
        {
            var provider = ScopeProvider;
            Assert.Throws<ArgumentException>(() =>
            {
                using (var scope = provider.CreateScope())
                {
                    scope.WriteLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void WriteLockExisting()
        {
            var provider = ScopeProvider;
            using (var scope = provider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }
    }
}
