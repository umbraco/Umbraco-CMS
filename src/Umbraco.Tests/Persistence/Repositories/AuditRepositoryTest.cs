using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class AuditRepositoryTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Add_Audit_Entry()
        {
            var provider = TestObjects.GetScopeUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var repo = new AuditRepository(unitOfWork, CacheHelper, Logger);
                repo.AddOrUpdate(new AuditItem(-1, "This is a System audit trail", AuditType.System, 0));
                unitOfWork.Complete();

                var dtos = unitOfWork.Database.Fetch<LogDto>("WHERE id > -1");

                Assert.That(dtos.Any(), Is.True);
                Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
            }
        }
    }
}