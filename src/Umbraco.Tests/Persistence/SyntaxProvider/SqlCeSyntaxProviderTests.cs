using System;
using System.Collections.Generic;
using System.Data.Common;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Syntax.Create.Index;
using Umbraco.Core.Persistence.Migrations.Syntax.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.SyntaxProvider
{
    [TestFixture]
    public class SqlCeSyntaxProviderTests : BaseUsingSqlCeSyntax
    {

        [Test]
        public void Can_Generate_Delete_SubQuery_Statement()
        {
            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
            var subQuery = Sql()
                            .Select("DISTINCT cmsContentXml.nodeId")
                            .From<ContentXmlDto>()
                            .InnerJoin<NodeDto>()
                            .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                            .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType);
            
            var sqlOutput = SqlContext.SqlSyntax.GetDeleteSubquery("cmsContentXml", "nodeId", subQuery);

            Assert.AreEqual(@"DELETE FROM [cmsContentXml] WHERE [nodeId] IN (SELECT [nodeId] FROM (SELECT DISTINCT cmsContentXml.nodeId
FROM [cmsContentXml]
INNER JOIN [umbracoNode]
ON [cmsContentXml].[nodeId] = [umbracoNode].[id]
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
            var sqlSyntax = new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null));
            
            var indexDefinition = CreateIndexDefinition();
            indexDefinition.IndexType = IndexTypes.NonClustered;

            var actual = sqlSyntax.Format(indexDefinition);
            Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
        }

        [Test]
        public void Format_SqlServer_NonClusteredIndexDefinition_UsingIsClusteredFalse_AddsClusteredDirective()
        {
            var sqlSyntax = new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null));

            var indexDefinition = CreateIndexDefinition();
            indexDefinition.IsClustered = false;

            var actual = sqlSyntax.Format(indexDefinition);
            Assert.AreEqual("CREATE CLUSTERED INDEX [IX_A] ON [TheTable] ([A])", actual);
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_NonClustered_CreatesNonClusteredIndex()
        {
            var sqlSyntax = new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null));
            var logger = Mock.Of<ILogger>();
            var db = TestObjects.GetUmbracoSqlServerDatabase(logger);
            var context = new MigrationContext(db, logger);
            var createExpression = new CreateIndexExpression(context, new []{ DatabaseType.SqlServer2005 })
            {
                Index = { Name = "IX_A" }
            };
            var builder = new CreateIndexBuilder(createExpression);
            builder.OnTable("TheTable").OnColumn("A").Ascending().WithOptions().NonClustered();
            Assert.AreEqual("CREATE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", createExpression.ToString());
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_Unique_CreatesUniqueNonClusteredIndex()
        {
            var sqlSyntax = new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null));
            var logger = Mock.Of<ILogger>();
            var db = TestObjects.GetUmbracoSqlServerDatabase(logger);
            var context = new MigrationContext(db, logger);
            var createExpression = new CreateIndexExpression(context, new[] { DatabaseType.SqlServer2005 })
            {
                Index = { Name = "IX_A" }
            };
            var builder = new CreateIndexBuilder(createExpression);
            builder.OnTable("TheTable").OnColumn("A").Ascending().WithOptions().Unique();
            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_A] ON [TheTable] ([A])", createExpression.ToString());
        }

        [Test]
        public void CreateIndexBuilder_SqlServer_Clustered_CreatesClusteredIndex()
        {
            var sqlSyntax = new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null));
            var logger = Mock.Of<ILogger>();
            var db = TestObjects.GetUmbracoSqlServerDatabase(logger);
            var context = new MigrationContext(db, logger);
            var createExpression = new CreateIndexExpression(context, new[] { DatabaseType.SqlServer2005 })
            {
                Index = { Name = "IX_A" }
            };
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
    }
}