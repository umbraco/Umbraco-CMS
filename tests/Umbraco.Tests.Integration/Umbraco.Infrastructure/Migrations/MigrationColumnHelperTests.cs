// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations;

/// <summary>
/// Tests that the schema inspecting column helpers of <see cref="AsyncMigrationBase" /> leave the migration's
/// transaction in a state where it commits, whether or not the inspected column exists.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class MigrationColumnHelperTests : UmbracoIntegrationTest
{
    private const string TableName = "migrationColumnHelperTest";

    [Test]
    public async Task Can_Commit_Migration_After_Checking_Existing_Column_Exists()
        => await AssertMigrationCommitsAsync<ColumnExistsForExistingColumnMigration>();

    [Test]
    public async Task Can_Commit_Migration_After_Checking_Missing_Column_Exists()
        => await AssertMigrationCommitsAsync<ColumnExistsForMissingColumnMigration>();

    [Test]
    public async Task Can_Commit_Migration_After_Getting_Existing_Column_Type()
        => await AssertMigrationCommitsAsync<ColumnTypeForExistingColumnMigration>();

    [Test]
    public async Task Can_Commit_Migration_After_Adding_Existing_Column()
        => await AssertMigrationCommitsAsync<AddExistingColumnMigration>();

    [Test]
    public async Task Can_Commit_Migration_After_Adding_Existing_Column_With_Deferred_Statements()
        => await AssertMigrationCommitsAsync<AddExistingColumnWithDeferredStatementsMigration>();

    private async Task AssertMigrationCommitsAsync<TMigration>()
        where TMigration : ColumnHelperMigrationBase
    {
        MigrationPlan plan = new MigrationPlan("test")
            .From(string.Empty)
            .To<TMigration>("done");

        ExecutedMigrationPlan result = await CreateExecutor().ExecutePlanAsync(plan, string.Empty);

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());

        // The table is created by the migration itself, so it only exists afterwards if the migration's transaction committed.
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
        Assert.That(database.SqlContext.SqlSyntax.DoesTableExist(database, TableName), Is.True);
        scope.Complete();
    }

    private MigrationPlanExecutor CreateExecutor() => new(
        GetRequiredService<ICoreScopeProvider>(),
        ScopeAccessor,
        LoggerFactory,
        GetRequiredService<IMigrationBuilder>(),
        GetRequiredService<IUmbracoDatabaseFactory>(),
        GetRequiredService<IDatabaseCacheRebuilder>(),
        GetRequiredService<DistributedCache>(),

        // Not the real IKeyValueService: this fixture runs against an empty database, so persisting the plan state
        // would fail on the missing umbracoLock table.
        Mock.Of<IKeyValueService>(),
        GetRequiredService<IServiceScopeFactory>(),
        AppCaches.NoCache,
        GetRequiredService<IPublishedContentTypeFactory>());

    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    public class TestDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_migrationColumnHelperTest")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }

    public abstract class ColumnHelperMigrationBase : AsyncMigrationBase
    {
        protected ColumnHelperMigrationBase(IMigrationContext context)
            : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            Create.Table<TestDto>().Do();
            InspectColumns();
            return Task.CompletedTask;
        }

        protected abstract void InspectColumns();
    }

    public class ColumnExistsForExistingColumnMigration : ColumnHelperMigrationBase
    {
        public ColumnExistsForExistingColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void InspectColumns() => Assert.That(ColumnExists(TableName, "id"), Is.True);
    }

    public class ColumnExistsForMissingColumnMigration : ColumnHelperMigrationBase
    {
        public ColumnExistsForMissingColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void InspectColumns() => Assert.That(ColumnExists(TableName, "missing"), Is.False);
    }

    public class ColumnTypeForExistingColumnMigration : ColumnHelperMigrationBase
    {
        public ColumnTypeForExistingColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void InspectColumns() => Assert.That(ColumnType(TableName, "id"), Is.Not.Null);
    }

    public class AddExistingColumnMigration : ColumnHelperMigrationBase
    {
        public AddExistingColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void InspectColumns() => Assert.That(AddColumn<TestDto>("name"), Is.False);
    }

    public class AddExistingColumnWithDeferredStatementsMigration : ColumnHelperMigrationBase
    {
        public AddExistingColumnWithDeferredStatementsMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void InspectColumns() => Assert.That(AddColumn<TestDto>("name", out _), Is.False);
    }
}
