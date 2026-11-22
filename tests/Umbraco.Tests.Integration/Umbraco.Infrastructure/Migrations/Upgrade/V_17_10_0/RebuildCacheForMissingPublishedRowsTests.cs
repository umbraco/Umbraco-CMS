// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_10_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V17_10_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RebuildCacheForMissingPublishedRowsTests : UmbracoIntegrationTestWithContent
{
    private IMigrationBuilder MigrationBuilder => GetRequiredService<IMigrationBuilder>();

    private IUmbracoDatabaseFactory UmbracoDatabaseFactory => GetRequiredService<IUmbracoDatabaseFactory>();

    private IServiceScopeFactory ServiceScopeFactory => GetRequiredService<IServiceScopeFactory>();

    private DistributedCache DistributedCache => GetRequiredService<DistributedCache>();

    private IDatabaseCacheRebuilder DatabaseCacheRebuilder => GetRequiredService<IDatabaseCacheRebuilder>();

    private IPublishedContentTypeFactory PublishedContentTypeFactory => GetRequiredService<IPublishedContentTypeFactory>();

    private IMigrationPlanExecutor MigrationPlanExecutor => new MigrationPlanExecutor(
        ScopeProvider,
        ScopeAccessor,
        LoggerFactory,
        MigrationBuilder,
        UmbracoDatabaseFactory,
        DatabaseCacheRebuilder,
        DistributedCache,
        Mock.Of<IKeyValueService>(),
        ServiceScopeFactory,
        AppCaches.NoCache,
        PublishedContentTypeFactory);
    [Test]
    public async Task Requests_Rebuild_When_A_Published_Document_Has_No_Published_Cache_Row()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        DeletePublishedCacheRow(Textpage.Id);

        Assert.That(await ExecuteMigration(), Is.True);
    }

    [Test]
    public async Task Requests_Rebuild_When_A_Published_Document_Below_The_Top_Of_The_Recycle_Bin_Has_No_Published_Cache_Row()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        Assert.That(ContentService.Publish(Subpage2, ["*"]).Success, Is.True);
        Assert.That(ContentService.MoveToRecycleBin(Textpage).Success, Is.True);
        DeletePublishedCacheRow(Subpage2.Id);

        Assert.That(await ExecuteMigration(), Is.True);
    }

    [Test]
    public async Task Does_Not_Request_Rebuild_When_Only_A_Published_Document_At_The_Top_Of_The_Recycle_Bin_Has_No_Published_Cache_Row()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        Assert.That(ContentService.MoveToRecycleBin(Textpage).Success, Is.True);
        DeletePublishedCacheRow(Textpage.Id);

        Assert.That(await ExecuteMigration(), Is.False);
    }

    [Test]
    public async Task Does_Not_Request_Rebuild_When_A_Document_Flagged_As_Published_Has_No_Published_Version()
    {
        using (IScope scope = ScopeProvider.CreateScope())
        {
            Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
                .Update<DocumentDto>(update => update.Set(x => x.Published, true))
                .Where<DocumentDto>(x => x.NodeId == Textpage.Id);
            scope.Database.Execute(sql);
            scope.Complete();
        }

        Assert.That(CountPublishedCacheRows(Textpage.Id), Is.EqualTo(0));
        Assert.That(await ExecuteMigration(), Is.False);
    }

    [Test]
    public async Task Does_Not_Request_Rebuild_When_Every_Published_Document_Has_A_Published_Cache_Row()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        Assert.That(CountPublishedCacheRows(Textpage.Id), Is.EqualTo(1));

        Assert.That(await ExecuteMigration(), Is.False);
    }

    [Test]
    public async Task Does_Not_Request_Rebuild_When_An_Unpublished_Document_Still_Has_A_Published_Version()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        Assert.That(ContentService.Unpublish(Textpage).Success, Is.True);
        Assert.That(CountPublishedCacheRows(Textpage.Id), Is.Zero);

        Assert.That(await ExecuteMigration(), Is.False);
    }

    [Test]
    public async Task Rebuilds_Missing_Published_Cache_Row_Through_Migration_Plan_Executor()
    {
        Assert.That(ContentService.Publish(Textpage, ["*"]).Success, Is.True);
        DeletePublishedCacheRow(Textpage.Id);
        Assert.That(CountPublishedCacheRows(Textpage.Id), Is.Zero);

        var upgrader = new Upgrader(
            new MigrationPlan("RebuildCacheForMissingPublishedRowsTest")
                .From(string.Empty)
                .To<RebuildCacheForMissingPublishedRows>("done"));

        ExecutedMigrationPlan result = await upgrader.ExecuteAsync(
            MigrationPlanExecutor,
            ScopeProvider,
            Mock.Of<IKeyValueService>());

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(CountPublishedCacheRows(Textpage.Id), Is.EqualTo(1));
        });
    }

    private void DeletePublishedCacheRow(int nodeId)
    {
        using IScope scope = ScopeProvider.CreateScope();
        Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
            .Delete<ContentNuDto>()
            .Where<ContentNuDto>(x => x.NodeId == nodeId && x.Published);
        scope.Database.Execute(sql);
        scope.Complete();
    }

    private int CountPublishedCacheRows(int nodeId)
    {
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
            .SelectCount()
            .From<ContentNuDto>()
            .Where<ContentNuDto>(x => x.NodeId == nodeId && x.Published);
        return scope.Database.ExecuteScalar<int>(sql);
    }

    private async Task<bool> ExecuteMigration()
    {
        using IScope scope = ScopeProvider.CreateScope();
        var context = new MigrationContext(new TestPlan(), scope.Database, GetRequiredService<ILogger<MigrationContext>>());
        var migration = new RebuildCacheForMissingPublishedRows(context);
        await migration.RunAsync();
        scope.Complete();
        return migration.RebuildCache;
    }

    private sealed class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("test-plan")
        {
        }
    }
}
