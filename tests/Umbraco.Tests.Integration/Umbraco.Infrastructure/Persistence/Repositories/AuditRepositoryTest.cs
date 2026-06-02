// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class AuditRepositoryTest : UmbracoIntegrationTest
{
    private ILogger<AuditRepository> _logger;

    [SetUp]
    public void Prepare() => _logger = LoggerFactory.CreateLogger<AuditRepository>();

    private IAuditItem GetAuditItem(int id) => new AuditItem(id, AuditType.System, -1, UmbracoObjectTypes.Document.GetName(), "This is a System audit trail");

    private AuditRepository CreateRepository() =>
        new(
            StaticServiceProvider.Instance.GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
            _logger,
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());

    [Test]
    public async Task Can_Add_Audit_Entry()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        await repo.SaveAsync(
            new AuditItem(-1, AuditType.System, -1, UmbracoObjectTypes.Document.GetName(), "This is a System audit trail"),
            CancellationToken.None);

        var page = await repo.GetPagedAsync(0, 10, Direction.Descending);

        Assert.That(page.Items.Any(), Is.True);
        Assert.That(page.Items.First().Comment, Is.EqualTo("This is a System audit trail"));

        scope.Complete();
    }

    [Test]
    public async Task Has_Create_Date_When_Get_By_Id()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        await repo.SaveAsync(GetAuditItem(1), CancellationToken.None);
        var auditEntry = await repo.GetAsync(1, CancellationToken.None);

        Assert.That(auditEntry, Is.Not.Null);
        Assert.That(auditEntry!.CreateDate, Is.Not.EqualTo(default(DateTime)));

        scope.Complete();
    }

    [Test]
    public async Task Has_Create_Date_When_Get_By_Paged_For_Entity()
    {
        using var scope = NewScopeProvider.CreateScope();
        var repo = CreateRepository();

        await repo.SaveAsync(GetAuditItem(1), CancellationToken.None);

        var page = await repo.GetPagedForEntityAsync(1, 0, 10, Direction.Ascending);
        var auditEntry = page.Items.FirstOrDefault();

        Assert.That(auditEntry, Is.Not.Null);
        Assert.That(auditEntry!.CreateDate, Is.Not.EqualTo(default(DateTime)));

        scope.Complete();
    }

    [Test]
    public async Task Get_Paged_Items()
    {
        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();

            for (var i = 0; i < 100; i++)
            {
                await repo.SaveAsync(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"), CancellationToken.None);
                await repo.SaveAsync(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"), CancellationToken.None);
            }

            scope.Complete();
        }

        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();

            var page = await repo.GetPagedAsync(0, 10, Direction.Descending);

            Assert.AreEqual(10, page.Items.Count());
            Assert.AreEqual(200, page.Total);

            scope.Complete();
        }
    }

    [Test]
    public async Task Get_Paged_Items_For_User()
    {
        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();

            for (var i = 0; i < 100; i++)
            {
                await repo.SaveAsync(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"), CancellationToken.None);
                await repo.SaveAsync(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"), CancellationToken.None);
            }

            scope.Complete();
        }

        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();

            var page = await repo.GetPagedForUserAsync(
                userId: -1,
                skip: 0,
                take: 10,
                orderDirection: Direction.Descending,
                auditTypeFilter: new[] { AuditType.Publish });

            Assert.AreEqual(10, page.Items.Count());
            Assert.AreEqual(100, page.Total);

            scope.Complete();
        }
    }

    [Test]
    public async Task Get_Paged_Items_With_AuditType_Filter()
    {
        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();

            for (var i = 0; i < 100; i++)
            {
                await repo.SaveAsync(new AuditItem(i, AuditType.New, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} created"), CancellationToken.None);
                await repo.SaveAsync(new AuditItem(i, AuditType.Publish, -1, UmbracoObjectTypes.Document.GetName(), $"Content {i} published"), CancellationToken.None);
            }

            scope.Complete();
        }

        using (var scope = NewScopeProvider.CreateScope())
        {
            var repo = CreateRepository();

            var page = await repo.GetPagedAsync(
                skip: 0,
                take: 9,
                orderDirection: Direction.Descending,
                auditTypeFilter: new[] { AuditType.Publish });

            var items = page.Items.ToArray();

            Assert.AreEqual(9, items.Length);
            Assert.IsTrue(items.All(x => x.AuditType == AuditType.Publish));
            Assert.AreEqual(100, page.Total);

            scope.Complete();
        }
    }
}
