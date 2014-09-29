using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class PetaPocoSqlTests : BaseUsingSqlCeSyntax
    {
        //x => 

        [Test]
        public void Where_Clause_With_Starts_With_Additional_Parameters()
        {
            var content = new NodeDto() { NodeId = 123, Path = "-1,123" };
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Path.SqlStartsWith(content.Path, TextColumnType.NVarchar));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[path]) LIKE upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(content.Path + "%", sql.Arguments[0]);        
        }

        [Test]
        public void Where_Clause_With_Starts_With_By_Variable()
        {
            var content = new NodeDto() {NodeId = 123, Path = "-1,123"};
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Path.StartsWith(content.Path) && x.NodeId != content.NodeId);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[path]) LIKE upper(@0) AND [umbracoNode].[id] <> @1)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
            Assert.AreEqual(content.NodeId, sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_Not_Starts_With()
        {
            var level = 1;
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Level == level && !x.Path.StartsWith("-20"));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[level] = @0 AND NOT (upper([umbracoNode].[path]) LIKE upper(@1)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(level, sql.Arguments[0]);
            Assert.AreEqual("-20%", sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_Equals_Clause()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Text.Equals("Hello@world.com"));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) = upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("Hello@world.com", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_False_Boolean()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => !x.Trashed);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (NOT ([umbracoNode].[trashed] = @0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(true, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Boolean()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Trashed);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(true, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_ToUpper()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Text.ToUpper() == "hello".ToUpper());

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) = upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("hello", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_ToString()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Text == 1.ToString());

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[text] = @0)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("1", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Wildcard()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Text.StartsWith("D"));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) LIKE upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("D%", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_Single_Constant()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.NodeId == 2);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[id] = @0)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(2, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_And_Constant()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.NodeId != 2 && x.NodeId != 3);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[id] <> @0 AND [umbracoNode].[id] <> @1)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(2, sql.Arguments[0]);
            Assert.AreEqual(3, sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_Or_Constant()
        {
            var sql = new Sql("SELECT *").From<NodeDto>().Where<NodeDto>(x => x.Text == "hello" || x.NodeId == 3);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[text] = @0 OR [umbracoNode].[id] = @1)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual("hello", sql.Arguments[0]);
            Assert.AreEqual(3, sql.Arguments[1]);
        }

        [Test]
        public void Can_Select_From_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>();

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_InnerJoin_With_Types()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]")
                .On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]");

            var sql = new Sql();
            sql.Select("*").From<DocumentDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_OrderBy_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").OrderBy("([cmsContent].[contentType])");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>().OrderBy<ContentDto>(x => x.ContentTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_GroupBy_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").GroupBy("[contentType]");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>().GroupBy<ContentDto>(x => x.ContentTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_Predicate()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").Where("[cmsContent].[nodeId] = @0", 1045);

            var sql = new Sql();
            sql.Select("*").From<ContentDto>().Where<ContentDto>(x => x.NodeId == 1045);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_And_Predicate()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsContent]")
                .Where("[cmsContent].[nodeId] = @0", 1045)
                .Where("[cmsContent].[contentType] = @0", 1050);

            var sql = new Sql();
            sql.Select("*")
                .From<ContentDto>()
                .Where<ContentDto>(x => x.NodeId == 1045)
                .Where<ContentDto>(x => x.ContentTypeId == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }
    }
}