// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class AuditRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public void Prepare() => _logger = LoggerFactory.CreateLogger<AuditRepository>();

    private ILogger<AuditRepository> _logger;

    private IAuditRepository AuditRepository => GetRequiredService<IAuditRepository>();

    private IAuditItem GetAuditItem(int id) => new AuditItem(id, AuditType.System, -1, UmbracoObjectTypes.Document.GetName(), "This is a System audit trail");

    [Test]
    public void Can_Add_Audit_Entry()
    {
        var sp = ScopeProvider;
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new AuditRepository((IScopeAccessor)sp, _logger);
            repo.Save(new AuditItem(-1, AuditType.System, -1, UmbracoObjectTypes.Document.GetName(), "This is a System audit trail"));

            var dtos = ScopeAccessor.AmbientScope.Database.Fetch<LogDto>("WHERE id > -1");

            Assert.That(dtos.Any(), Is.True);
            Assert.That(dtos.First().Comment, Is.EqualTo("This is a System audit trail"));
        }
    }

    [Test]
    public void Has_Create_Date_When_Get_By_Id()
    {
        using var scope = ScopeProvider.CreateScope();

        AuditRepository.Save(GetAuditItem(1));
        var auditEntry = AuditRepository.Get(1);
        Assert.That(auditEntry.CreateDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void Has_Create_Date_When_Get_By_Query()
    {
        using var scope = ScopeProvider.CreateScope();

        AuditRepository.Save(GetAuditItem(1));
        var auditEntry = AuditRepository.Get(AuditType.System, ScopeProvider.CreateQuery<IAuditItem>().Where(x => x.Id == 1)).FirstOrDefault();
        Assert.That(auditEntry, Is.Not.Null);
        Assert.That(auditEntry.CreateDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public void Has_Create_Date_When_Get_By_Paged_Query()
    {
        using var scope = ScopeProvider.CreateScope();

        AuditRepository.Save(GetAuditItem(1));
        var auditEntry = AuditRepository.GetPagedResultsByQuery(ScopeProvider.CreateQuery<IAuditItem>().Where(x => x.Id == 1),0, 10, out long total, Direction.Ascending, null, null).FirstOrDefault();
        Assert.That(auditEntry, Is.Not.Null);
        Assert.That(auditEntry.CreateDate, Is.Not.EqualTo(default(DateTime)));
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

            var page = repo.GetPagedResultsByQuery(sp.CreateQuery<IAuditItem>(), 0, 10, out var total, Direction.Descending, null, null);

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

            var query = sp.CreateQuery<IAuditItem>().Where(x => x.UserId == -1);

            try
            {
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                var page = repo.GetPagedResultsByQuery(
                    query,
                    0,
                    10,
                    out var total,
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

            var page = repo.GetPagedResultsByQuery(
                    sp.CreateQuery<IAuditItem>(),
                    0,
                    9,
                    out var total,
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

            var page = repo.GetPagedResultsByQuery(
                    sp.CreateQuery<IAuditItem>(),
                    0,
                    8,
                    out var total,
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
