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
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_5_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V18_5_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class FixMemberEmptyStringDefaultsTests : UmbracoIntegrationTest
{
    private const string TableName = "cmsMember";
    private const string EmptyStringDefinition = "('')";
    private static readonly string[] _columnNames = ["Email", "LoginName", "Password"];

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
    public void Fresh_Install_Has_Empty_String_Defaults()
    {
        if (SkipOnSqlite())
        {
            return;
        }

        AssertEmptyStringDefaults();
    }

    [Test]
    public async Task Can_Migrate_Single_Apostrophe_Defaults()
    {
        if (SkipOnSqlite())
        {
            return;
        }

        // Recreate the defaults as previous versions created them.
        foreach (var columnName in _columnNames)
        {
            ExecuteNonQuery(
                $"ALTER TABLE [{TableName}] DROP CONSTRAINT [DF_{TableName}_{columnName}];" +
                $"ALTER TABLE [{TableName}] ADD CONSTRAINT [DF_{TableName}_{columnName}] DEFAULT ('''') FOR [{columnName}];");
        }

        Assert.That(
            GetDefaults().Values.Select(x => x.Definition),
            Is.All.EqualTo("('''')"),
            "Arrange failed: the defaults should be a single apostrophe, as created by previous versions.");

        ExecutedMigrationPlan result = await ExecuteMigration();

        AssertMigrated(result);
    }

    [Test]
    public async Task Can_Migrate_When_Default_Constraints_Have_System_Generated_Names()
    {
        if (SkipOnSqlite())
        {
            return;
        }

        // Omitting the constraint name has SQL Server generate one, as happens on databases whose schema was
        // materialised outside of Umbraco.
        foreach (var columnName in _columnNames)
        {
            ExecuteNonQuery(
                $"ALTER TABLE [{TableName}] DROP CONSTRAINT [DF_{TableName}_{columnName}];" +
                $"ALTER TABLE [{TableName}] ADD DEFAULT ('''') FOR [{columnName}];");
        }

        ExecutedMigrationPlan result = await ExecuteMigration();

        AssertMigrated(result);
    }

    [Test]
    public async Task Can_Migrate_When_Default_Constraints_Are_Missing()
    {
        if (SkipOnSqlite())
        {
            return;
        }

        foreach (var columnName in _columnNames)
        {
            ExecuteNonQuery($"ALTER TABLE [{TableName}] DROP CONSTRAINT [DF_{TableName}_{columnName}];");
        }

        Assert.That(GetDefaults(), Is.Empty, "Arrange failed: the constraints should have been removed.");

        ExecutedMigrationPlan result = await ExecuteMigration();

        AssertMigrated(result);
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

    private void AssertMigrated(ExecutedMigrationPlan result)
    {
        // The upgrader captures rather than throws migration exceptions, so a plan that failed would otherwise
        // go unnoticed here.
        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
        AssertEmptyStringDefaults();
    }

    private void AssertEmptyStringDefaults()
    {
        Dictionary<string, DefaultConstraint> defaults = GetDefaults();

        Assert.Multiple(() =>
        {
            foreach (var columnName in _columnNames)
            {
                Assert.That(defaults.TryGetValue(columnName, out DefaultConstraint? constraint), Is.True, $"{columnName} has no default constraint");
                Assert.That(constraint?.Name, Is.EqualTo($"DF_{TableName}_{columnName}"));
                Assert.That(constraint?.Definition, Is.EqualTo(EmptyStringDefinition), columnName);
            }
        });
    }

    private async Task<ExecutedMigrationPlan> ExecuteMigration()
    {
        var upgrader = new Upgrader(
            new MigrationPlan("FixMemberEmptyStringDefaultsTest")
                .From(string.Empty)
                .To<FixMemberEmptyStringDefaults>("done"));

        return await upgrader.ExecuteAsync(MigrationPlanExecutor, ScopeProvider, Mock.Of<IKeyValueService>());
    }

    private Dictionary<string, DefaultConstraint> GetDefaults()
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();

        // Queried directly rather than through the syntax provider, so that the assertion doesn't share a code path
        // with the migration it is verifying.
        return db.Fetch<DefaultConstraint>(
                @"SELECT col.[name] AS [ColumnName], dc.[name] AS [Name], dc.[definition] AS [Definition]
FROM sys.default_constraints dc
JOIN sys.tables tbl ON tbl.object_id = dc.parent_object_id
JOIN sys.columns col ON col.object_id = dc.parent_object_id AND col.column_id = dc.parent_column_id
JOIN sys.schemas s ON s.[schema_id] = tbl.[schema_id]
WHERE s.[name] = SCHEMA_NAME() AND tbl.[name] = @0;",
                TableName)
            .Where(x => _columnNames.Contains(x.ColumnName))
            .ToDictionary(x => x.ColumnName);
    }

    private void ExecuteNonQuery(string sql)
    {
        using IUmbracoDatabase db = UmbracoDatabaseFactory.CreateDatabase();
        db.Execute(sql);
    }

    private sealed class DefaultConstraint
    {
        public string ColumnName { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string Definition { get; set; } = null!;
    }
}
