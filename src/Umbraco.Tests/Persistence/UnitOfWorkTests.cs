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
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            Assert.Throws<Exception>(() =>
            {
                using (var uow = provider.CreateUnitOfWork())
                {
                    scope.ReadLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void ReadLockExisting()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var uow = provider.CreateUnitOfWork())
            {
                scope.ReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void WriteLockNonExisting()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            Assert.Throws<Exception>(() =>
            {
                using (var uow = provider.CreateUnitOfWork())
                {
                    scope.WriteLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void WriteLockExisting()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var uow = provider.CreateUnitOfWork())
            {
                scope.WriteLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }
    }
}
