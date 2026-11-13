// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

/// <summary>
///     A database can store a shorter name than the one requested for an index, as the syntax provider's
///     <see cref="ISqlSyntaxProvider.TruncateConstraintName{T}" /> describes. Looking an index up by its requested
///     name must still find it.
/// </summary>
[TestFixture]
public class ShortenedIndexNameTests
{
    private const string TableName = "umbTestShortenedIndexName";

    private const string IndexName = "IX_umbTestShortenedIndexName_name_with_a_long_name";

    [Test]
    public async Task Migrations_Find_An_Index_Whose_Name_The_Database_Shortened()
    {
        var database = new TestDatabase(syntaxProvider: new ShortIdentifierSyntaxProvider());
        var migration = new IndexExistsMigration(new MigrationContext(new TestPlan(), database, Mock.Of<ILogger<MigrationContext>>()));

        await migration.RunAsync();

        Assert.That(migration.IndexExisted, Is.True);
    }

    [Test]
    public void Schema_Validation_Accepts_An_Index_Whose_Name_The_Database_Shortened()
    {
        var database = new TestDatabase(syntaxProvider: new ShortIdentifierSyntaxProvider());
        var schemaCreator = new DatabaseSchemaCreator(
            database,
            NullLogger<DatabaseSchemaCreator>.Instance,
            NullLoggerFactory.Instance,
            Mock.Of<IUmbracoVersion>(),
            Mock.Of<IEventAggregator>(),
            Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>());

        DatabaseSchemaResult result = schemaCreator.ValidateSchema([typeof(ShortenedIndexNameDto)]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Errors.Where(x => x.Item1 == "Index"), Is.Empty);
            Assert.That(result.ValidIndexes, Has.Count.EqualTo(1));
        });
    }

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("Test")
        {
        }
    }

    private sealed class IndexExistsMigration : AsyncMigrationBase
    {
        public IndexExistsMigration(IMigrationContext context)
            : base(context)
        {
        }

        public bool? IndexExisted { get; private set; }

        protected override Task MigrateAsync()
        {
            IndexExisted = IndexExists(IndexName);
            return Task.CompletedTask;
        }
    }

    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    private sealed class ShortenedIndexNameDto
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public int Id { get; set; }

        [Column("name")]
        [Index(IndexTypes.NonClustered, Name = IndexName)]
        public string Name { get; set; } = string.Empty;
    }

    // Stands for databases with a shorter identifier limit, which store the index under its shortened name.
    private sealed class ShortIdentifierSyntaxProvider : SqlServerSyntaxProvider
    {
        private const int MaxIdentifierLength = 30;

        public ShortIdentifierSyntaxProvider()
            : base(Options.Create(new GlobalSettings()))
        {
        }

        public override string TruncateConstraintName<T>(string constraintName)
            => constraintName.Length > MaxIdentifierLength ? constraintName[..MaxIdentifierLength] : constraintName;

        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db)
            => [Tuple.Create(TableName, TruncateConstraintName<IndexDefinition>(IndexName), "name", false)];

        public override IEnumerable<string> GetTablesInSchema(IDatabase db) => [TableName];

        public override IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db) => [];

        public override IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db) => [];

        public override IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db) => [];
    }
}
