using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class UnitOfWorkTests : BaseDatabaseFactoryTest
    {
        [Test]
        public void ReadLockNonExisting()
        {
            var provider = new NPocoUnitOfWorkProvider(Logger, MappingResolver);
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
            var provider = new NPocoUnitOfWorkProvider(Logger, MappingResolver);
            using (var uow = provider.CreateUnitOfWork())
            {
                uow.ReadLock(Constants.Locks.Servers);
                uow.Complete();
            }
        }

        [Test]
        public void WriteLockNonExisting()
        {
            var provider = new NPocoUnitOfWorkProvider(Logger, MappingResolver);
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
            var provider = new NPocoUnitOfWorkProvider(Logger, MappingResolver);
            using (var uow = provider.CreateUnitOfWork())
            {
                uow.WriteLock(Constants.Locks.Servers);
                uow.Complete();
            }
        }
    }
}
