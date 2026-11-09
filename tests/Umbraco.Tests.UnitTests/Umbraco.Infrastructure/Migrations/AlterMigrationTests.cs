// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Stubs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

[TestFixture]
public class AlterMigrationTests
{
    private readonly ILogger<MigrationContext> _logger = Mock.Of<ILogger<MigrationContext>>();

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("Test")
        {
        }
    }

    private MigrationContext GetMigrationContext(out TestDatabase db)
    {
        db = new TestDatabase();
        return new MigrationContext(new TestPlan(), db, _logger);
    }

    [Test]
    public async Task Drop_Foreign_Key()
    {
        // Arrange
        var context = GetMigrationContext(out var database);
        var stub = new DropForeignKeyMigrationStub(context);

        // Act
        await stub.RunAsync().ConfigureAwait(false);

        foreach (var op in database.Operations)
        {
            Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);
        }

        // Assert
        Assert.That(database.Operations.Count, Is.EqualTo(1));
        Assert.That(database.Operations[0].Sql, Is.EqualTo("ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoUser_id]"));
    }

    [Test]
    public async Task CreateColumn()
    {
        var context = GetMigrationContext(out var database);
        var migration = new CreateColumnMigration(context);

        await migration.RunAsync().ConfigureAwait(false);

        foreach (var op in database.Operations)
        {
            Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);
        }

        Assert.That(database.Operations.Count, Is.EqualTo(1));
        Assert.That(
            database.Operations[0].Sql,
            Is.EqualTo("ALTER TABLE [bar] ADD [foo] UniqueIdentifier NOT NULL"));
    }

    public class CreateColumnMigration : AsyncMigrationBase
    {
        public CreateColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override async Task MigrateAsync()
        {
            Alter.Table("bar").AddColumn("foo").AsGuid().Do();
        }
    }

    [Test]
    public async Task AlterColumn()
    {
        var context = GetMigrationContext(out var database);
        var migration = new AlterColumnMigration(context);

        await migration.RunAsync().ConfigureAwait(false);

        foreach (var op in database.Operations)
        {
            Console.WriteLine("{0}\r\n\t{1}", op.Text, op.Sql);
        }

        Assert.That(database.Operations.Count, Is.EqualTo(1));
        Assert.That(
            database.Operations[0].Sql,
            Is.EqualTo("ALTER TABLE [bar] ALTER COLUMN [foo] UniqueIdentifier NOT NULL"));
    }

    [Test]
    public async Task AlterColumn_Uses_The_Statement_Of_The_Syntax_Provider()
    {
        var database = new TestDatabase(syntaxProvider: new SeparateClausesAlterColumnSyntaxProvider());
        var migration = new AlterColumnMigration(new MigrationContext(new TestPlan(), database, _logger));

        await migration.RunAsync().ConfigureAwait(false);

        Assert.That(database.Operations.Count, Is.EqualTo(1));
        Assert.That(
            database.Operations[0].Sql,
            Is.EqualTo("ALTER TABLE [bar] ALTER COLUMN [foo] TYPE UniqueIdentifier, ALTER COLUMN [foo] SET NOT NULL"));
    }

    public class AlterColumnMigration : AsyncMigrationBase
    {
        public AlterColumnMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override Task MigrateAsync()
        {
            // bad/good syntax...
            //// Alter.Column("foo").OnTable("bar").AsGuid().NotNullable();
            Alter.Table("bar").AlterColumn("foo").AsGuid().NotNullable().Do();
            return Task.CompletedTask;
        }
    }

    // Stands for databases that change a column's type and nullability in separate clauses.
    private sealed class SeparateClausesAlterColumnSyntaxProvider : SqlServerSyntaxProvider
    {
        public SeparateClausesAlterColumnSyntaxProvider()
            : base(Options.Create(new GlobalSettings()))
        {
        }

        public override string FormatAlterColumn(string? tableName, ColumnDefinition column)
        {
            var columnName = GetQuotedColumnName(column.Name);
            var nullability = column.IsNullable ? "DROP NOT NULL" : "SET NOT NULL";
            return $"ALTER TABLE {GetQuotedTableName(tableName)} ALTER COLUMN {columnName} TYPE {FormatType(column)}, ALTER COLUMN {columnName} {nullability}";
        }
    }
}
