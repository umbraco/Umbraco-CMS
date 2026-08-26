using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V17_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class SetDateDefaultsToUtcNowTests : UmbracoIntegrationTest
{
    private const string TableName = "umbracoNode";
    private const string ColumnName = "createDate";
    private const string ExpectedConstraintName = "DF_umbracoNode_createDate";

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
    public async Task Can_Migrate_When_Default_Constraint_Has_The_Expected_Name()
    {
        if (SkipOnSqlite())
        {
            return;
        }

        Assert.That(
            GetDefaultConstraint()?.Name,
            Is.EqualTo(ExpectedConstraintName),
            "Arrange failed: a freshly created schema should carry the conventionally named constraint.");

        ExecutedMigrationPlan result = await ExecuteMigration();

        AssertMigratedToUtcDefault(result);
    }

    [Test]
    public async Task Can_Migrate_When_Default_Constraint_Has_A_System_Generated_Name()
    {
        if (SkipOnSqlite())
        {
            return;
        }

        // Omitting the constraint name has SQL Server generate one, as happens on databases whose schema was
        // materialised outside of Umbraco.
        ExecuteNonQuery(
            $"ALTER TABLE [{TableName}] DROP CONSTRAINT [{ExpectedConstraintName}];" +
            $"ALTER TABLE [{TableName}] ADD DEFAULT (getdate()) FOR [{ColumnName}];");

        Assert.That(
            GetDefaultConstraint()?.Name,
            Is.Not.Null.And.Not.EqualTo(ExpectedConstraintName),
            "Arrange failed: the constraint should have been replaced by a system generated one.");

        ExecutedMigrationPlan result = await ExecuteMigration();

        AssertMigratedToUtcDefault(result);
    }

    [Test]
    public async Task Can_Migrate_When_Default_Constraint_Is_Missing()
    {
        if (SkipOnSqlite())
        {
            return;
        }

        ExecuteNonQuery($"ALTER TABLE [{TableName}] DROP CONSTRAINT [{ExpectedConstraintName}];");

        Assert.That(
            GetDefaultConstraint(),
            Is.Null,
            "Arrange failed: the constraint should have been removed.");

        ExecutedMigrationPlan result = await ExecuteMigration();

        AssertMigratedToUtcDefault(result);
    }

    private static bool SkipOnSqlite()
    {
        if (BaseTestDatabase.IsSqlite() is false)
        {
            return false;
        }

        Assert.Ignore("Named default constraints are a SQL Server concept, so the migration is a no-op on SQLite.");
        return true;
    }

    private void AssertMigratedToUtcDefault(ExecutedMigrationPlan result)
    {
        // The upgrader captures rather than throws migration exceptions, so a plan that failed would otherwise
        // go unnoticed here.
        Assert.That(result.Successful, Is.True, result.Exception?.ToString());

        DefaultConstraint? constraint = GetDefaultConstraint();

        Assert.Multiple(() =>
        {
            Assert.That(constraint?.Name, Is.EqualTo(ExpectedConstraintName));
            Assert.That(constraint?.Definition, Does.Contain("getutcdate").IgnoreCase);
        });
    }

    private async Task<ExecutedMigrationPlan> ExecuteMigration()
    {
        var upgrader = new Upgrader(
            new MigrationPlan("SetDateDefaultsToUtcNowTest")
                .From(string.Empty)
                .To<SetDateDefaultsToUtcNow>("done"));

        return await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
    }

    private DefaultConstraint? GetDefaultConstraint()
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();

        // Queried directly rather than through the syntax provider, so that the assertion doesn't share a code path
        // with the migration it is verifying.
        return db.Fetch<DefaultConstraint>(
            @"SELECT dc.[name] AS [Name], dc.[definition] AS [Definition]
FROM sys.default_constraints dc
JOIN sys.tables tbl ON tbl.object_id = dc.parent_object_id
JOIN sys.columns col ON col.object_id = dc.parent_object_id AND col.column_id = dc.parent_column_id
JOIN sys.schemas s ON s.[schema_id] = tbl.[schema_id]
WHERE s.[name] = SCHEMA_NAME() AND tbl.[name] = @0 AND col.[name] = @1;",
            TableName,
            ColumnName).FirstOrDefault();
    }

    private void ExecuteNonQuery(string sql)
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();
        db.Execute(sql);
    }

    private sealed class DefaultConstraint
    {
        public string Name { get; set; } = null!;

        public string Definition { get; set; } = null!;
    }
}
