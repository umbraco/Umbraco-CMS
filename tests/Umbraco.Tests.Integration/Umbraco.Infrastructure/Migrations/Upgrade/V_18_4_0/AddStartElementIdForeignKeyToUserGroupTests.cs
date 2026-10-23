using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_4_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_18_4_0;

/// <summary>
/// Tests that <see cref="AddStartElementIdForeignKeyToUserGroup" /> gives the element start node column the same
/// foreign key to <c>umbracoNode</c> that a clean install creates, keeping the start element ids that reference a node.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class AddStartElementIdForeignKeyToUserGroupTests : UmbracoIntegrationTest
{
    private const int ExistingNodeId = 1234;
    private const int MissingNodeId = 5678;

    [Test]
    public async Task Keeps_Start_Element_Ids_That_Reference_A_Node()
    {
        CreateTables(ForeignKeyDeclaration.None);
        InsertUserGroup(1, ExistingNodeId);
        InsertUserGroup(2, null);

        await ExecuteMigrationAsync();

        Assert.Multiple(() =>
        {
            Assert.That(GetStartElementId(1), Is.EqualTo(ExistingNodeId));
            Assert.That(GetStartElementId(2), Is.Null);
        });
    }

    [Test]
    public async Task Clears_Start_Element_Ids_That_Reference_A_Missing_Node()
    {
        CreateTables(ForeignKeyDeclaration.None);
        InsertUserGroup(1, MissingNodeId);

        await ExecuteMigrationAsync();

        Assert.That(GetStartElementId(1), Is.Null);
    }

    [Test]
    public async Task Start_Element_Id_Must_Reference_A_Node()
    {
        CreateTables(ForeignKeyDeclaration.None);

        await ExecuteMigrationAsync();

        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => InsertUserGroup(1, ExistingNodeId));
            Assert.Catch(() => InsertUserGroup(2, MissingNodeId));
        });
    }

    [TestCase(ForeignKeyDeclaration.OnTable)]
    [TestCase(ForeignKeyDeclaration.OnColumn)]
    public async Task Leaves_An_Existing_Foreign_Key_In_Place(ForeignKeyDeclaration foreignKeyDeclaration)
    {
        CreateTables(foreignKeyDeclaration);
        InsertUserGroup(1, ExistingNodeId);

        await ExecuteMigrationAsync();

        Assert.Multiple(() =>
        {
            Assert.That(GetStartElementId(1), Is.EqualTo(ExistingNodeId));
            Assert.Catch(() => InsertUserGroup(2, MissingNodeId));
        });
    }

    public enum ForeignKeyDeclaration
    {
        None,
        OnTable,
        OnColumn,
    }

    private void CreateTables(ForeignKeyDeclaration foreignKeyDeclaration)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        ISqlSyntaxProvider sqlSyntax = database.SqlContext.SqlSyntax;

        var nodeTable = sqlSyntax.GetQuotedTableName(NodeDto.TableName);
        var nodeId = sqlSyntax.GetQuotedColumnName(NodeDto.PrimaryKeyColumnName);
        var startElementId = sqlSyntax.GetQuotedColumnName(UserGroupDto.StartElementIdColumnName);
        var references = $"REFERENCES {nodeTable} ({nodeId})";
        var startElementIdColumn = foreignKeyDeclaration switch
        {
            ForeignKeyDeclaration.OnTable => $"{startElementId} INTEGER NULL, CONSTRAINT {UserGroupDto.StartElementIdForeignKeyName} FOREIGN KEY ({startElementId}) {references}",
            ForeignKeyDeclaration.OnColumn => $"{startElementId} INTEGER NULL CONSTRAINT {UserGroupDto.StartElementIdForeignKeyName} {references}",
            _ => $"{startElementId} INTEGER NULL",
        };

        database.Execute($"CREATE TABLE {nodeTable} ({nodeId} INTEGER NOT NULL PRIMARY KEY)");
        database.Execute(
            $"CREATE TABLE {sqlSyntax.GetQuotedTableName(UserGroupDto.TableName)} ({sqlSyntax.GetQuotedColumnName(UserGroupDto.PrimaryKeyColumnName)} INTEGER NOT NULL PRIMARY KEY, {startElementIdColumn})");
        database.Execute($"INSERT INTO {nodeTable} ({nodeId}) VALUES ({ExistingNodeId})");

        scope.Complete();
    }

    private void InsertUserGroup(int id, int? startElementId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        ISqlSyntaxProvider sqlSyntax = database.SqlContext.SqlSyntax;

        database.Execute(
            $"INSERT INTO {sqlSyntax.GetQuotedTableName(UserGroupDto.TableName)} ({sqlSyntax.GetQuotedColumnName(UserGroupDto.PrimaryKeyColumnName)}, {sqlSyntax.GetQuotedColumnName(UserGroupDto.StartElementIdColumnName)}) VALUES (@0, @1)",
            id,
            startElementId);

        scope.Complete();
    }

    private int? GetStartElementId(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        ISqlSyntaxProvider sqlSyntax = database.SqlContext.SqlSyntax;

        int? startElementId = database.ExecuteScalar<int?>(
            $"SELECT {sqlSyntax.GetQuotedColumnName(UserGroupDto.StartElementIdColumnName)} FROM {sqlSyntax.GetQuotedTableName(UserGroupDto.TableName)} WHERE {sqlSyntax.GetQuotedColumnName(UserGroupDto.PrimaryKeyColumnName)} = @0",
            id);

        scope.Complete();
        return startElementId;
    }

    private async Task ExecuteMigrationAsync()
    {
        MigrationPlan plan = new MigrationPlan(nameof(AddStartElementIdForeignKeyToUserGroupTests))
            .From(string.Empty)
            .To<AddStartElementIdForeignKeyToUserGroup>("done");

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
