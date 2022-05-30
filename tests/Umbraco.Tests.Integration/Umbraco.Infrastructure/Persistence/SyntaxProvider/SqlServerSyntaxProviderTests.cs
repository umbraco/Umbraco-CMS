using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.SyntaxProvider;

[TestFixture]
public class SqlServerSyntaxProviderTests : UmbracoIntegrationTest
{
    private ISqlContext SqlContext => GetRequiredService<IUmbracoDatabaseFactory>().SqlContext;
    private SqlServerSyntaxProvider GetSqlSyntax() => new(Options.Create(new GlobalSettings()));

    private class TestPlan : MigrationPlan
    {
        public TestPlan() : base("Test")
        {
        }
    }

    private MigrationContext GetMigrationContext(out TestDatabase db)
    {
        var logger = Mock.Of<ILogger<MigrationContext>>();
        var sqlSyntax = GetSqlSyntax();
        db = new TestDatabase(DatabaseType.SqlServer2005, sqlSyntax);
        return new MigrationContext(new TestPlan(), db, logger);
    }

    [UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
    [Test]
    public void Can_Generate_Delete_SubQuery_Statement()
    {
        var mediaObjectType = Constants.ObjectTypes.Media;
        var subQuery = SqlContext.Sql()
            .Select("DISTINCT cmsContentNu.nodeId")
            .From<ContentNuDto>()
            .InnerJoin<NodeDto>()
            .On<ContentNuDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType);

        var sqlOutput = SqlContext.SqlSyntax.GetDeleteSubquery("cmsContentNu", "nodeId", subQuery);

        string t(string x)
        {
            return SqlContext.SqlSyntax.GetQuotedTableName(x);
        }

        string c(string x)
        {
            return SqlContext.SqlSyntax.GetQuotedColumnName(x);
        }

        Assert.AreEqual(
            @$"DELETE FROM {t("cmsContentNu")} WHERE {c("nodeId")} IN (SELECT {c("nodeId")} FROM (SELECT DISTINCT cmsContentNu.nodeId
FROM {t("cmsContentNu")}
INNER JOIN {t("umbracoNode")}
ON {t("cmsContentNu")}.{c("nodeId")} = {t("umbracoNode")}.{c("id")}
WHERE (({t("umbracoNode")}.{c("nodeObjectType")} = @0))) x)".Replace(Environment.NewLine, " ").Replace("\n", " ")
                .Replace("\r", " "),
            sqlOutput.SQL.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " "));

        Assert.AreEqual(1, sqlOutput.Arguments.Length);
        Assert.AreEqual(mediaObjectType, sqlOutput.Arguments[0]);
    }

    [NUnit.Framework.Ignore("This doesn't actually test anything")]
    [Test]
    public void Can_Generate_Create_Table_Statement()
    {
        var type = typeof(NodeDto);
        var definition = DefinitionFactory.GetTableDefinition(type, SqlContext.SqlSyntax);

        var create = SqlContext.SqlSyntax.Format(definition);
        var primaryKey = SqlContext.SqlSyntax.FormatPrimaryKey(definition);
        var indexes = SqlContext.SqlSyntax.Format(definition.Indexes);
        var keys = SqlContext.SqlSyntax.Format(definition.ForeignKeys);

        Debug.Print(create);
        Debug.Print(primaryKey);
        foreach (var sql in keys)
        {
            Debug.Print(sql);
        }

        foreach (var sql in indexes)
        {
            Debug.Print(sql);
        }
    }

    [Test]
    public void Format_SqlServer_NonClusteredIndexDefinition_AddsNonClusteredDirective()
    {
        var sqlSyntax = GetSqlSyntax();

        var indexDefinition = CreateIndexDefinition();
        indexDefinition.IndexType = IndexTypes.NonClustered;

        var actual = sqlSyntax.Format(indexDefinition);
        Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
    }

    [Test]
    public void Format_SqlServer_NonClusteredIndexDefinition_UsingIsClusteredFalse_AddsClusteredDirective()
    {
        var sqlSyntax = GetSqlSyntax();

        var indexDefinition = CreateIndexDefinition();
        indexDefinition.IndexType = IndexTypes.Clustered;

        var actual = sqlSyntax.Format(indexDefinition);
        Assert.AreEqual("CREATE CLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
    }

    [Test]
    public void CreateIndexBuilder_SqlServer_NonClustered_CreatesNonClusteredIndex()
    {
        var context = GetMigrationContext(out var db);

        var createExpression = new CreateIndexExpression(context) { Index = { Name = "IX_A" } };

        new CreateIndexBuilder(createExpression)
            .OnTable("TheTable").OnColumn("A").Ascending().WithOptions().NonClustered()
            .Do();

        Assert.AreEqual(1, db.Operations.Count);
        Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", db.Operations[0].Sql);
    }

    [Test]
    public void CreateIndexBuilder_SqlServer_Unique_CreatesUniqueNonClusteredIndex()
    {
        var context = GetMigrationContext(out var db);

        var createExpression = new CreateIndexExpression(context) { Index = { Name = "IX_A" } };

        new CreateIndexBuilder(createExpression)
            .OnTable("TheTable").OnColumn("A").Ascending().WithOptions().Unique()
            .Do();

        Assert.AreEqual(1, db.Operations.Count);
        Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", db.Operations[0].Sql);
    }

    [Test]
    public void CreateIndexBuilder_SqlServer_Unique_CreatesUniqueNonClusteredIndex_Multi_Columnn()
    {
        var context = GetMigrationContext(out var db);

        var createExpression = new CreateIndexExpression(context) { Index = { Name = "IX_AB" } };

        new CreateIndexBuilder(createExpression)
            .OnTable("TheTable").OnColumn("A").Ascending().OnColumn("B").Ascending().WithOptions().Unique()
            .Do();

        Assert.AreEqual(1, db.Operations.Count);
        Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_AB] ON [TheTable] ([A],[B])", db.Operations[0].Sql);
    }

    [Test]
    public void CreateIndexBuilder_SqlServer_Clustered_CreatesClusteredIndex()
    {
        var context = GetMigrationContext(out var db);

        var createExpression = new CreateIndexExpression(context) { Index = { Name = "IX_A" } };

        new CreateIndexBuilder(createExpression)
            .OnTable("TheTable").OnColumn("A").Ascending().WithOptions().Clustered()
            .Do();

        Assert.AreEqual(1, db.Operations.Count);
        Assert.AreEqual("CREATE CLUSTERED INDEX [IX_A] ON [TheTable] ([A])", db.Operations[0].Sql);
    }

    private static IndexDefinition CreateIndexDefinition() =>
        new IndexDefinition { ColumnName = "A", Name = "IX_A", TableName = "TheTable", SchemaName = "dbo" };
}
