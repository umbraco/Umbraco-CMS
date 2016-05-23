using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class AuditRepositoryTest : BaseDatabaseFactoryTest
    {
        [Test]
        public void Can_Add_Audit_Entry()
        {
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, MappingResolver);
                repo.AddOrUpdate(new AuditItem(-1, "This is a System audit trail", AuditType.System, 0));
                unitOfWork.Complete();
            }

            var dtos = DatabaseContext.Database.Fetch<LogDto>("WHERE id > -1");

            Assert.That(dtos.Any(), Is.True);
            Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
        }
    }
}