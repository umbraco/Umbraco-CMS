using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.SyntaxProvider
{
    [TestFixture]
    public class SqlServerSyntaxProviderTests : UmbracoIntegrationTest
    {
        private ISqlContext SqlContext => GetRequiredService<IUmbracoDatabaseFactory>().SqlContext;

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

            Assert.AreEqual(@"DELETE FROM [cmsContentNu] WHERE [nodeId] IN (SELECT [nodeId] FROM (SELECT DISTINCT cmsContentNu.nodeId
FROM [cmsContentNu]
INNER JOIN [umbracoNode]
ON [cmsContentNu].[nodeId] = [umbracoNode].[id]
WHERE (([umbracoNode].[nodeObjectType] = @0))) x)".Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " "),
                                                sqlOutput.SQL.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " "));

            Assert.AreEqual(1, sqlOutput.Arguments.Length);
            Assert.AreEqual(mediaObjectType, sqlOutput.Arguments[0]);
        }

        [NUnit.Framework.Ignore("This doesn't actually test anything")]
        [Test]
        public void Can_Generate_Create_Table_Statement()
        {
            var type = typeof (NodeDto);
            var definition = DefinitionFactory.GetTableDefinition(type, SqlContext.SqlSyntax);

            string create = SqlContext.SqlSyntax.Format(definition);
            string primaryKey = SqlContext.SqlSyntax.FormatPrimaryKey(definition);
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
            var sqlSyntax = new SqlServerSyntaxProvider();

            var indexDefinition = CreateIndexDefinition();
            indexDefinition.IndexType = IndexTypes.NonClustered;

            var actual = sqlSyntax.Format(indexDefinition);
            Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
        }

        [Test]
        public void Format_SqlServer_NonClusteredIndexDefinition_UsingIsClusteredFalse_AddsClusteredDirective()
        {
            var sqlSyntax = new SqlServerSyntaxProvider();

            var indexDefinition = CreateIndexDefinition();
            indexDefinition.IndexType = IndexTypes.Clustered;

            var actual = sqlSyntax.Format(indexDefinition);
            Assert.AreEqual("CREATE CLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_NonClustered_CreatesNonClusteredIndex()
        {
            var logger = Mock.Of<ILogger<MigrationContext>>();
            var sqlSyntax = new SqlServerSyntaxProvider();
            var db = new TestDatabase(DatabaseType.SqlServer2005, sqlSyntax);
            var context = new MigrationContext(db, logger);

            var createExpression = new CreateIndexExpression(context)
            {
                Index = { Name = "IX_A" }
            };

            new CreateIndexBuilder(createExpression)
                .OnTable("TheTable").OnColumn("A").Ascending().WithOptions().NonClustered()
                .Do();

            Assert.AreEqual(1, db.Operations.Count);
            Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", db.Operations[0].Sql);
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_Unique_CreatesUniqueNonClusteredIndex()
        {
            var logger = Mock.Of<ILogger<MigrationContext>>();
            var sqlSyntax = new SqlServerSyntaxProvider();
            var db = new TestDatabase(DatabaseType.SqlServer2005, sqlSyntax);
            var context = new MigrationContext(db, logger);

            var createExpression = new CreateIndexExpression(context)
            {
                Index = { Name = "IX_A" }
            };

            new CreateIndexBuilder(createExpression)
                .OnTable("TheTable").OnColumn("A").Ascending().WithOptions().Unique()
                .Do();

            Assert.AreEqual(1, db.Operations.Count);
            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", db.Operations[0].Sql);
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_Unique_CreatesUniqueNonClusteredIndex_Multi_Columnn()
        {
            var logger = Mock.Of<ILogger<MigrationContext>>();
            var sqlSyntax = new SqlServerSyntaxProvider();
            var db = new TestDatabase(DatabaseType.SqlServer2005, sqlSyntax);
            var context = new MigrationContext(db, logger);

            var createExpression = new CreateIndexExpression(context)
            {
                Index = { Name = "IX_AB" }
            };

            new CreateIndexBuilder(createExpression)
                .OnTable("TheTable").OnColumn("A").Ascending().OnColumn("B").Ascending().WithOptions().Unique()
                .Do();

            Assert.AreEqual(1, db.Operations.Count);
            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_AB] ON [TheTable] ([A],[B])", db.Operations[0].Sql);
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_Clustered_CreatesClusteredIndex()
        {
            var logger = Mock.Of<ILogger<MigrationContext>>();
            var sqlSyntax = new SqlServerSyntaxProvider();
            var db = new TestDatabase(DatabaseType.SqlServer2005, sqlSyntax);
            var context = new MigrationContext(db, logger);

            var createExpression = new CreateIndexExpression(context)
            {
                Index = { Name = "IX_A" }
            };

            new CreateIndexBuilder(createExpression)
                .OnTable("TheTable").OnColumn("A").Ascending().WithOptions().Clustered()
                .Do();

            Assert.AreEqual(1, db.Operations.Count);
            Assert.AreEqual("CREATE CLUSTERED INDEX [IX_A] ON [TheTable] ([A])", db.Operations[0].Sql);
        }

        private static IndexDefinition CreateIndexDefinition()
        {
            return new IndexDefinition
            {
                ColumnName = "A",
                Name = "IX_A",
                TableName = "TheTable",
                SchemaName = "dbo"
            };
        }
    }
}
