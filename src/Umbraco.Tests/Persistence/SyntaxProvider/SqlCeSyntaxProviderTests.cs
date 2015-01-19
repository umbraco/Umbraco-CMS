using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Index;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.SyntaxProvider
{
    [TestFixture]
    public class SqlCeSyntaxProviderTests
    {
        [SetUp]
        public void SetUp()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();
        }

        [Test]
        public void Can_Generate_Delete_SubQuery_Statement()
        {
            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
            var subQuery = new Sql()
                            .Select("DISTINCT cmsContentXml.nodeId")
                            .From<ContentXmlDto>()
                            .InnerJoin<NodeDto>()
                            .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                            .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType);
            
            var sql = SqlSyntaxContext.SqlSyntaxProvider.GetDeleteSubquery("cmsContentXml", "nodeId", subQuery);

            Assert.AreEqual(@"DELETE FROM [cmsContentXml] WHERE [nodeId] IN (SELECT [nodeId] FROM (SELECT DISTINCT cmsContentXml.nodeId
FROM [cmsContentXml]
INNER JOIN [umbracoNode]
ON [cmsContentXml].[nodeId] = [umbracoNode].[id]
WHERE ([umbracoNode].[nodeObjectType] = @0)) x)".Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " "), 
                                                sql.SQL.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " "));

            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(mediaObjectType, sql.Arguments[0]);
        }

        [NUnit.Framework.Ignore("This doesn't actually test anything")]
        [Test]
        public void Can_Generate_Create_Table_Statement()
        {
            var type = typeof (NodeDto);
            var definition = DefinitionFactory.GetTableDefinition(type);

            string create = SqlSyntaxContext.SqlSyntaxProvider.Format(definition);
            string primaryKey = SqlSyntaxContext.SqlSyntaxProvider.FormatPrimaryKey(definition);
            var indexes = SqlSyntaxContext.SqlSyntaxProvider.Format(definition.Indexes);
            var keys = SqlSyntaxContext.SqlSyntaxProvider.Format(definition.ForeignKeys);

            Console.WriteLine(create);
            Console.WriteLine(primaryKey);
            foreach (var sql in keys)
            {
                Console.WriteLine(sql);
            }

            foreach (var sql in indexes)
            {
                Console.WriteLine(sql);
            }
        }

        [Test]
        public void Format_SqlServer_NonClusteredIndexDefinition_AddsNonClusteredDirective()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new SqlServerSyntaxProvider();

            var indexDefinition = CreateIndexDefinition();
            indexDefinition.IndexType = IndexTypes.NonClustered;

            var actual = SqlSyntaxContext.SqlSyntaxProvider.Format(indexDefinition);
            Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
        }

        [Test]
        public void Format_SqlServer_NonClusteredIndexDefinition_UsingIsClusteredFalse_AddsClusteredDirective()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new SqlServerSyntaxProvider();

            var indexDefinition = CreateIndexDefinition();
            indexDefinition.IsClustered = false;

            var actual = SqlSyntaxContext.SqlSyntaxProvider.Format(indexDefinition);
            Assert.AreEqual("CREATE CLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_NonClustered_CreatesNonClusteredIndex()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new SqlServerSyntaxProvider();
            var createExpression = new CreateIndexExpression { Index = { Name = "IX_A" } };
            var builder = new CreateIndexBuilder(createExpression);
            builder.OnTable("TheTable").OnColumn("A").Ascending().WithOptions().NonClustered();
            Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", createExpression.ToString());
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_Unique_CreatesUniqueNonClusteredIndex()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new SqlServerSyntaxProvider();
            var createExpression = new CreateIndexExpression { Index = { Name = "IX_A" } };
            var builder = new CreateIndexBuilder(createExpression);
            builder.OnTable("TheTable").OnColumn("A").Ascending().WithOptions().Unique();
            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", createExpression.ToString());
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_Clustered_CreatesClusteredIndex()
        {
            SqlSyntaxContext.SqlSyntaxProvider = new SqlServerSyntaxProvider();
            var createExpression = new CreateIndexExpression { Index = { Name = "IX_A" } };
            var builder = new CreateIndexBuilder(createExpression);
            builder.OnTable("TheTable").OnColumn("A").Ascending().WithOptions().Clustered();
            Assert.AreEqual("CREATE CLUSTERED INDEX [IX_A] ON [TheTable] ([A])", createExpression.ToString());
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

        [TearDown]
        public void TearDown()
        {
            SqlSyntaxContext.SqlSyntaxProvider = null;
        }
    }
}