using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using umbraco;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;

using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;
using umbraco.editorControls.tinyMCE3;
using umbraco.interfaces;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class AuditRepositoryTest : BaseDatabaseFactoryTest
    {
        [Test]
        public void Can_Add_Audit_Entry()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                repo.AddOrUpdate(new AuditItem(-1, "This is a System audit trail", AuditType.System, 0));
                unitOfWork.Commit();
            }

            var dtos = DatabaseContext.Database.Fetch<LogDto>("WHERE id > -1");

            Assert.That(dtos.Any(), Is.True);
            Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
        }
    }
}