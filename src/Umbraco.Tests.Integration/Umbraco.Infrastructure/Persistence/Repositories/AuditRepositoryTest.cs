﻿using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;
using Microsoft.Extensions.Logging;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
    public class AuditRepositoryTest : UmbracoIntegrationTest
    {
        private ILogger<AuditRepository> _logger;

        [SetUp]
        public void Prepare()
        {
            _logger = LoggerFactory.CreateLogger<AuditRepository>();
        }

        [Test]
        public void Can_Add_Audit_Entry()
        {
            var sp = ScopeProvider;
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);
                repo.Save(new AuditItem(-1, AuditType.System, -1, UmbracoObjectTypes.Document.GetName(), "This is a System audit trail"));

                var dtos = scope.Database.Fetch<LogDto>("WHERE id > -1");

                Assert.That(dtos.Any(), Is.True);
                Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
            }
        }

        [Test]
        public void Get_Paged_Items()
        {
            var sp = ScopeProvider;
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (var i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"));
                }

                scope.Complete();
            }

            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                var page = repo.GetPagedResultsByQuery(sp.SqlContext.Query<IAuditItem>(), 0, 10, out var total, Direction.Descending, null, null);

                Assert.AreEqual(10, page.Count());
                Assert.AreEqual(200, total);
            }
        }

        [Test]
        public void Get_Paged_Items_By_User_Id_With_Query_And_Filter()
        {
            var sp = ScopeProvider;
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (var i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"));
                }

                scope.Complete();
            }

            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                var query = sp.SqlContext.Query<IAuditItem>().Where(x => x.UserId == -1);

                try
                {
                    scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    scope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                    var page = repo.GetPagedResultsByQuery(query, 0, 10, out var total, Direction.Descending,
                            new[] { AuditType.Publish },
                            sp.SqlContext.Query<IAuditItem>().Where(x => x.UserId > -2));

                    Assert.AreEqual(10, page.Count());
                    Assert.AreEqual(100, total);
                }
                finally
                {
                    scope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                    scope.Database.AsUmbracoDatabase().EnableSqlCount = false;
                }
            }
        }

        [Test]
        public void Get_Paged_Items_With_AuditType_Filter()
        {
            var sp = ScopeProvider;
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (var i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"));
                }

                scope.Complete();
            }

            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                var page = repo.GetPagedResultsByQuery(sp.SqlContext.Query<IAuditItem>(), 0, 9, out var total, Direction.Descending,
                        new[] { AuditType.Publish }, null)
                    .ToArray();

                Assert.AreEqual(9, page.Length);
                Assert.IsTrue(page.All(x => x.AuditType == AuditType.Publish));
                Assert.AreEqual(100, total);
            }
        }

        [Test]
        public void Get_Paged_Items_With_Custom_Filter()
        {
            var sp = ScopeProvider;
            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (var i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), "Content created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), "Content published"));
                }

                scope.Complete();
            }

            using (var scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                var page = repo.GetPagedResultsByQuery(sp.SqlContext.Query<IAuditItem>(), 0, 8, out var total, Direction.Descending,
                        null, sp.SqlContext.Query<IAuditItem>().Where(item => item.Comment == "Content created"))
                    .ToArray();

                Assert.AreEqual(8, page.Length);
                Assert.IsTrue(page.All(x => x.Comment == "Content created"));
                Assert.AreEqual(100, total);
            }
        }
    }
}
