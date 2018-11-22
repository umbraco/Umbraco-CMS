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

        [Test]
        public void Get_Paged_Items()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                for (int i = 0; i < 100; i++)
                {
                    repo.AddOrUpdate(new AuditItem(i, string.Format("Content {0} created", i), AuditType.New, 0));
                    repo.AddOrUpdate(new AuditItem(i, string.Format("Content {0} published", i), AuditType.Publish, 0));
                }
                unitOfWork.Commit();
            }

            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                long total;
                var page = repo.GetPagedResultsByQuery(Query<IAuditItem>.Builder, 0, 10, out total, Direction.Descending, null, null);

                Assert.AreEqual(10, page.Count());
                Assert.AreEqual(200, total);
            }
        }

        [Test]
        public void Get_Paged_Items_By_User_Id_With_Query_And_Filter()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                for (int i = 0; i < 100; i++)
                {
                    repo.AddOrUpdate(new AuditItem(i, string.Format("Content {0} created", i), AuditType.New, 0));
                    repo.AddOrUpdate(new AuditItem(i, string.Format("Content {0} published", i), AuditType.Publish, 0));
                }
                unitOfWork.Commit();
            }

            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                var query = Query<IAuditItem>.Builder.Where(x => x.UserId == 0);

                try
                {
                    DatabaseContext.Database.EnableSqlTrace = true;
                    DatabaseContext.Database.EnableSqlCount();

                    var page = repo.GetPagedResultsByQuery(query, 0, 10, out var total, Direction.Descending,
                            new[] { AuditType.Publish },
                            Query<IAuditItem>.Builder.Where(x => x.UserId > -1));

                    Assert.AreEqual(10, page.Count());
                    Assert.AreEqual(100, total);
                }
                finally
                {
                    DatabaseContext.Database.EnableSqlTrace = false;
                    DatabaseContext.Database.DisableSqlCount();
                }
            }
        }


        [Test]
        public void Get_Paged_Items_With_AuditType_Filter()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                for (int i = 0; i < 100; i++)
                {
                    repo.AddOrUpdate(new AuditItem(i, string.Format("Content {0} created", i), AuditType.New, 0));
                    repo.AddOrUpdate(new AuditItem(i, string.Format("Content {0} published", i), AuditType.Publish, 0));
                }
                unitOfWork.Commit();
            }

            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                try
                {
                    DatabaseContext.Database.EnableSqlTrace = true;
                    DatabaseContext.Database.EnableSqlCount();

                    var page = repo.GetPagedResultsByQuery(Query<IAuditItem>.Builder, 0, 9, out var total, Direction.Descending,
                                new[] { AuditType.Publish }, null)
                            .ToArray();

                    Assert.AreEqual(9, page.Length);
                    Assert.IsTrue(page.All(x => x.AuditType == AuditType.Publish));
                    Assert.AreEqual(100, total);
                }
                finally
                {
                    DatabaseContext.Database.EnableSqlTrace = false;
                    DatabaseContext.Database.DisableSqlCount();
                }
            }
        }

        [Test]
        public void Get_Paged_Items_With_Custom_Filter()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                for (int i = 0; i < 100; i++)
                {
                    repo.AddOrUpdate(new AuditItem(i, "Content created", AuditType.New, 0));
                    repo.AddOrUpdate(new AuditItem(i, "Content published", AuditType.Publish, 0));
                }
                unitOfWork.Commit();
            }

            using (var repo = new AuditRepository(unitOfWork, CacheHelper, Logger, SqlSyntax))
            {
                try
                {
                    DatabaseContext.Database.EnableSqlTrace = true;
                    DatabaseContext.Database.EnableSqlCount();

                    var page = repo.GetPagedResultsByQuery(Query<IAuditItem>.Builder, 0, 8, out var total, Direction.Descending,
                                null, Query<IAuditItem>.Builder.Where(item => item.Comment == "Content created"))
                            .ToArray();

                    Assert.AreEqual(8, page.Length);
                    Assert.IsTrue(page.All(x => x.Comment == "Content created"));
                    Assert.AreEqual(100, total);
                }
                finally
                {
                    DatabaseContext.Database.EnableSqlTrace = false;
                    DatabaseContext.Database.DisableSqlCount();
                }
            }
        }
    }
}
