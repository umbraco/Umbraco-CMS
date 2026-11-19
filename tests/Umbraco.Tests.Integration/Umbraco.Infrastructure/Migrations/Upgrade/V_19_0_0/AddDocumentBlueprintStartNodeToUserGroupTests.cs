using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Tests that <see cref="AddDocumentBlueprintStartNodeToUserGroup" /> adds the document blueprint start node column
/// with the same foreign key to <c>umbracoNode</c> that a clean install creates.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class AddDocumentBlueprintStartNodeToUserGroupTests : UmbracoIntegrationTest
{
    private const int ExistingNodeId = 1234;

    [SetUp]
    public void CreateTablesWithoutTheColumn()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        ISqlSyntaxProvider sqlSyntax = database.SqlContext.SqlSyntax;

        database.Execute($"CREATE TABLE {sqlSyntax.GetQuotedTableName(NodeDto.TableName)} ({sqlSyntax.GetQuotedColumnName(NodeDto.PrimaryKeyColumnName)} INTEGER NOT NULL PRIMARY KEY)");
        database.Execute($"CREATE TABLE {sqlSyntax.GetQuotedTableName(UserGroupDto.TableName)} ({sqlSyntax.GetQuotedColumnName(UserGroupDto.PrimaryKeyColumnName)} INTEGER NOT NULL PRIMARY KEY)");
        database.Execute($"INSERT INTO {sqlSyntax.GetQuotedTableName(NodeDto.TableName)} ({sqlSyntax.GetQuotedColumnName(NodeDto.PrimaryKeyColumnName)}) VALUES ({ExistingNodeId})");

        scope.Complete();
    }

    [Test]
    public async Task Can_Reference_An_Existing_Node()
    {
        await ExecuteMigrationAsync();

        Assert.DoesNotThrow(() => InsertUserGroup(1, ExistingNodeId));
    }

    [Test]
    public async Task Cannot_Reference_A_Missing_Node()
    {
        await ExecuteMigrationAsync();

        Assert.Catch(() => InsertUserGroup(1, ExistingNodeId + 1));
    }

    private void InsertUserGroup(int id, int startDocumentBlueprintId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        ISqlSyntaxProvider sqlSyntax = database.SqlContext.SqlSyntax;

        database.Execute(
            $"INSERT INTO {sqlSyntax.GetQuotedTableName(UserGroupDto.TableName)} ({sqlSyntax.GetQuotedColumnName(UserGroupDto.PrimaryKeyColumnName)}, {sqlSyntax.GetQuotedColumnName(UserGroupDto.StartDocumentBlueprintIdColumnName)}) VALUES (@0, @1)",
            id,
            startDocumentBlueprintId);

        scope.Complete();
    }

    private async Task ExecuteMigrationAsync()
    {
        MigrationPlan plan = new MigrationPlan(nameof(AddDocumentBlueprintStartNodeToUserGroupTests))
            .From(string.Empty)
            .To<AddDocumentBlueprintStartNodeToUserGroup>("done");

        var executor = new MigrationPlanExecutor(
            GetRequiredService<ICoreScopeProvider>(),
            ScopeAccessor,
            LoggerFactory,
            GetRequiredService<IMigrationBuilder>(),
            GetRequiredService<IUmbracoDatabaseFactory>(),
            GetRequiredService<IDatabaseCacheRebuilder>(),
            GetRequiredService<DistributedCache>(),
            Mock.Of<IKeyValueService>(),
            GetRequiredService<IServiceScopeFactory>(),
            AppCaches.NoCache,
            GetRequiredService<IPublishedContentTypeFactory>());

        ExecutedMigrationPlan result = await executor.ExecutePlanAsync(plan, string.Empty);

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
    }
}
