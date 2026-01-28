using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class SchemaValidationTest : UmbracoIntegrationTest
{
    private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();

    private IEventAggregator EventAggregator => GetRequiredService<IEventAggregator>();

    [Test]
    public void DatabaseSchemaCreation_Produces_DatabaseSchemaResult_With_Zero_Errors()
    {
        DatabaseSchemaResult result;

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            var schema = new DatabaseSchemaCreator(
                ScopeAccessor.AmbientScope.Database,
                LoggerFactory.CreateLogger<DatabaseSchemaCreator>(),
                LoggerFactory,
                UmbracoVersion,
                EventAggregator,
                Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>(x =>
                    x.CurrentValue == new InstallDefaultDataSettings()));
            schema.InitializeDatabaseSchema();
            result = schema.ValidateSchema(DatabaseSchemaCreator._orderedTables);
        }

        // Assert
        Assert.That(result.Errors.Count, Is.EqualTo(0));
    }

    [Test]
    public void Validate_DatabaseSchemaResult_with_CustomTables_has_Errors()
    {
        if (!IsSQLiteDatabase())
        {
            Assert.Ignore("This test is only valid for SQLite databases.");
            return;
        }

        DatabaseSchemaResult result;
        DatabaseSchemaCreator schema;

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            schema = new DatabaseSchemaCreator(
                ScopeAccessor.AmbientScope.Database,
                LoggerFactory.CreateLogger<DatabaseSchemaCreator>(),
                LoggerFactory,
                UmbracoVersion,
                EventAggregator,
                Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>(x =>
                    x.CurrentValue == new InstallDefaultDataSettings()));
            schema.InitializeDatabaseSchema();
        }

        // add some custom tables with columns, indexes, constraints, etc.
        CreateCustomTables();

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            result = schema.ValidateSchema(DatabaseSchemaCreator._orderedTables);
        }

        // Assert
        Assert.That(result.Errors.Count, Is.EqualTo(11));

        var expectedErrors = new List<Tuple<string, string>>()
        {
            Tuple.Create("Table", "customTable"),
            Tuple.Create("Column", "customTable,id"),
            Tuple.Create("Column", "customTable,name"),
            Tuple.Create("Index", "IX_customTable_name"),
            Tuple.Create("Constraint", "PK_customTable"),
            Tuple.Create("Table", "customTableFK"),
            Tuple.Create("Column", "customTableFK,id"),
            Tuple.Create("Column", "customTableFK,customTableId"),
            Tuple.Create("Column", "customTableFK,name"),
            Tuple.Create("Unknown", "CFK_customTableFK_id"),
            Tuple.Create("Constraint", "PK_customTableFK"),
        };
        foreach (var expectedError in expectedErrors)
        {
            Assert.That(result.Errors.Contains(expectedError), Is.True);
        }
    }

    private bool IsSQLiteDatabase()
    {
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            return ScopeAccessor.AmbientScope.Database.SqlContext.DatabaseType is NPoco.DatabaseTypes.SQLiteDatabaseType;
        }
    }

    /// <summary>
    /// Creates 'customTable' and 'customTableFK' tables in the specified Umbraco database.
    /// </summary>
    /// <remarks>
    /// This method is used to create custom tables for testing purposes only.
    /// </remarks>
    private void CreateCustomTables()
    {
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            string q(string name) => ScopeAccessor.AmbientScope.Database.SqlContext
                .SqlSyntax.GetQuotedName(name);

            ScopeAccessor.AmbientScope.Database
                .Execute(@$"
CREATE TABLE {q("customTable")} (
    {q("id")} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    {q("name")} NVARCHAR(255)
)");

            ScopeAccessor.AmbientScope.Database
                .Execute("CREATE INDEX IX_customTable_name ON customTable (name)");

            ScopeAccessor.AmbientScope.Database
                .Execute(@$"
CREATE TABLE {q("customTableFK")}
(
    {q("id")} INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    {q("customTableId")} INT NOT NULL,
    {q("name")} NVARCHAR(255),
    CONSTRAINT CFK_customTableFK_id FOREIGN KEY ({q("customTableId")}) REFERENCES {q("customTable")} ({q("id")})
)");
        }
    }
}
