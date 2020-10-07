using System;
using NUnit.Framework;
using Umbraco.Core;
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
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
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
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void WriteLockNonExisting()
        {
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
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
            var provider = TestObjects.GetScopeProvider(LoggerFactory);
            using (var scope = provider.CreateScope())
            {
                scope.WriteLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }
    }
}
