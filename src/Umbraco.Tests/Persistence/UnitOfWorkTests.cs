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
                    uow.ReadLock(-666);
                    uow.Complete();
                }
            });
        }

        [Test]
        public void ReadLockExisting()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var uow = provider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.Servers);
                uow.Complete();
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
                    uow.WriteLock(-666);
                    uow.Complete();
                }
            });
        }

        [Test]
        public void WriteLockExisting()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var uow = provider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.Servers);
                uow.Complete();
            }
        }
    }
}
