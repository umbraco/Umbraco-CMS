// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
    public class AuditRepositoryTest : UmbracoIntegrationTest
    {
        private ILogger<AuditRepository> _logger;

        [SetUp]
        public void Prepare() => _logger = LoggerFactory.CreateLogger<AuditRepository>();

        [Test]
        public void Can_Add_Audit_Entry()
        {
            IScopeProvider sp = ScopeProvider;
            using (IScope scope = ScopeProvider.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);
                repo.Save(new AuditItem(-1, AuditType.System, -1, UmbracoObjectTypes.Document.GetName(), "This is a System audit trail"));

                List<LogDto> dtos = ScopeAccessor.AmbientScope.Database.Fetch<LogDto>("WHERE id > -1");
                scope.Rollback();

                Assert.That(dtos.Any(), Is.True);
                Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
            }
        }

        [Test]
        public void Get_Paged_Items()
        {
            IScopeProvider sp = ScopeProvider;
            using (IScope scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (int i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"));
                }

                scope.Complete();
            }

            using (IScope scope = sp.CreateScope(autoComplete: true))
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                IEnumerable<IAuditItem> page = repo.GetPagedResultsByQuery(sp.CreateQuery<IAuditItem>(), 0, 10, out long total, Direction.Descending, null, null);

                Assert.AreEqual(10, page.Count());
                Assert.AreEqual(200, total);
            }
        }

        [Test]
        public void Get_Paged_Items_By_User_Id_With_Query_And_Filter()
        {
            IScopeProvider sp = ScopeProvider;
            using (IScope scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (int i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"));
                }

                scope.Complete();
            }

            using (IScope scope = sp.CreateScope(autoComplete: true))
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                IQuery<IAuditItem> query = sp.CreateQuery<IAuditItem>().Where(x => x.UserId == -1);

                try
                {
                    ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                    ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                    IEnumerable<IAuditItem> page = repo.GetPagedResultsByQuery(
                            query,
                            0,
                            10,
                            out long total,
                            Direction.Descending,
                            new[] { AuditType.Publish },
                            sp.CreateQuery<IAuditItem>()
                        .Where(x => x.UserId > -2));

                    Assert.AreEqual(10, page.Count());
                    Assert.AreEqual(100, total);
                }
                finally
                {
                    ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = false;
                    ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = false;
                }
            }
        }

        [Test]
        public void Get_Paged_Items_With_AuditType_Filter()
        {
            IScopeProvider sp = ScopeProvider;
            using (IScope scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (int i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"));
                }

                scope.Complete();
            }

            using (IScope scope = sp.CreateScope(autoComplete: true))
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                IAuditItem[] page = repo.GetPagedResultsByQuery(
                        sp.CreateQuery<IAuditItem>(),
                        0,
                        9,
                        out long total,
                        Direction.Descending,
                        new[] { AuditType.Publish },
                        null)
                    .ToArray();

                Assert.AreEqual(9, page.Length);
                Assert.IsTrue(page.All(x => x.AuditType == AuditType.Publish));
                Assert.AreEqual(100, total);
            }
        }

        [Test]
        public void Get_Paged_Items_With_Custom_Filter()
        {
            IScopeProvider sp = ScopeProvider;
            using (IScope scope = sp.CreateScope())
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                for (int i = 0; i < 100; i++)
                {
                    repo.Save(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), "Content created"));
                    repo.Save(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), "Content published"));
                }

                scope.Complete();
            }

            using (IScope scope = sp.CreateScope(autoComplete: true))
            {
                var repo = new AuditRepository((IScopeAccessor)sp, _logger);

                IAuditItem[] page = repo.GetPagedResultsByQuery(
                        sp.CreateQuery<IAuditItem>(),
                        0,
                        8,
                        out long total,
                        Direction.Descending,
                        null,
                        sp.CreateQuery<IAuditItem>()
                    .Where(item => item.Comment == "Content created"))
                    .ToArray();


                Assert.AreEqual(8, page.Length);
                Assert.IsTrue(page.All(x => x.Comment == "Content created"));
                Assert.AreEqual(100, total);
            }
        }
    }
}
