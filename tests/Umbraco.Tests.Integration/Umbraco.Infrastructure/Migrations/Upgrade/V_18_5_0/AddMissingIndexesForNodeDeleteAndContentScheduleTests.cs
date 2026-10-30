using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_5_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V18_5_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddMissingIndexesForNodeDeleteAndContentScheduleTests : UmbracoIntegrationTest
{
    private static readonly (string Table, string Index)[] _indexes =
    [
        (RelationDto.TableName, RelationDto.ChildIdIndexName),
        (ContentScheduleDto.TableName, ContentScheduleDto.NodeIdIndexName),
        (ContentScheduleDto.TableName, ContentScheduleDto.ActionDateIndexName),
        (RedirectUrlDto.TableName, RedirectUrlDto.ContentKeyIndexName),
        (User2NodeNotifyDto.TableName, User2NodeNotifyDto.NodeIdIndexName),
        (UserStartNodeDto.TableName, UserStartNodeDto.StartNodeIndexName),
    ];

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
    public void Fresh_Install_Has_The_Indexes()
        => Assert.That(GetDefinedIndexNames(), Is.SupersetOf(_indexes.Select(x => x.Index)));

    [Test]
    public async Task Can_Create_The_Indexes_When_Missing()
    {
        DropIndexes();
        Assert.That(
            GetDefinedIndexNames(),
            Has.None.AnyOf(_indexes.Select(x => x.Index).ToArray()),
            "Arrange failed: the indexes should have been dropped.");

        ExecutedMigrationPlan result = await ExecuteMigration();

        // The upgrader captures rather than throws migration exceptions, so a plan that failed would otherwise
        // go unnoticed here.
        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
        Assert.That(GetDefinedIndexNames(), Is.SupersetOf(_indexes.Select(x => x.Index)));
    }

    [Test]
    public async Task Can_Run_When_The_Indexes_Already_Exist()
    {
        ExecutedMigrationPlan result = await ExecuteMigration();

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
        Assert.That(GetDefinedIndexNames(), Is.SupersetOf(_indexes.Select(x => x.Index)));
    }

    [Test]
    public void Delete_Of_Relations_By_Child_Uses_The_Index()
    {
        if (BaseTestDatabase.IsSqlite())
        {
            Assert.Ignore("Verifies the SQL Server execution plan.");
        }

        using (ScopeProvider.CreateScope())
        {
            IUmbracoDatabase db = ScopeAccessor.AmbientScope!.Database;

            // Enough rows for the optimizer to prefer a seek over scanning the table.
            db.Execute(
                @"WITH n AS (SELECT TOP (2000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS i FROM sys.all_objects a CROSS JOIN sys.all_objects b)
                  INSERT INTO [umbracoNode] ([parentId], [level], [path], [sortOrder], [text], [nodeObjectType])
                  SELECT -1, 1, N'-1', i, CONCAT(N'node ', i), @0 FROM n",
                Constants.ObjectTypes.Document);
            var firstNodeId = db.ExecuteScalar<int>("SELECT MAX([id]) - 1999 FROM [umbracoNode]");
            var relationTypeId = db.ExecuteScalar<int>("SELECT MIN([id]) FROM [umbracoRelationType]");
            db.Execute(
                @"WITH n AS (SELECT TOP (50000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS i FROM sys.all_objects a CROSS JOIN sys.all_objects b)
                  INSERT INTO [umbracoRelation] ([parentId], [childId], [relType], [datetime], [comment])
                  SELECT @0 + i / 25, @0 + i % 2000, @1, GETUTCDATE(), N'' FROM n",
                firstNodeId,
                relationTypeId);
            db.Execute("UPDATE STATISTICS [umbracoRelation]");

            db.Execute("SET SHOWPLAN_XML ON");
            string plan;
            try
            {
                plan = db.ExecuteScalar<string>($"DELETE FROM [umbracoRelation] WHERE [childId] = {firstNodeId + 42}");
            }
            finally
            {
                db.Execute("SET SHOWPLAN_XML OFF");
            }

            Assert.That(plan, Does.Contain("Index Seek").And.Contain($"[{RelationDto.ChildIdIndexName}]"));
        }
    }

    private async Task<ExecutedMigrationPlan> ExecuteMigration()
    {
        var upgrader = new Upgrader(
            new MigrationPlan("AddMissingIndexesForNodeDeleteAndContentScheduleTest")
                .From(string.Empty)
                .To<AddMissingIndexesForNodeDeleteAndContentSchedule>("done"));

        return await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
    }

    private string[] GetDefinedIndexNames()
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();
        return db.SqlContext.SqlSyntax.GetDefinedIndexes(db).Select(x => x.Item2).Distinct().ToArray();
    }

    private void DropIndexes()
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();
        ISqlSyntaxProvider syntax = db.SqlContext.SqlSyntax;
        foreach ((string table, string index) in _indexes)
        {
            db.Execute(string.Format(syntax.DropIndex, syntax.GetQuotedName(index), syntax.GetQuotedTableName(table)));
        }
    }
}
