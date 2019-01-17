using NUnit.Framework;
using System.Diagnostics;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class PetaPocoSqlTests : BaseUsingSqlCeSyntax
    {
        private readonly ISqlSyntaxProvider _sqlSyntax = new SqlCeSyntaxProvider();

        [Test]
        public void Where_Clause_With_Starts_With_Additional_Parameters()
        {
            var content = new NodeDto() { NodeId = 123, Path = "-1,123" };
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Path.SqlStartsWith(content.Path, TextColumnType.NVarchar), _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[path]) LIKE upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Starts_With_By_Variable()
        {
            var content = new NodeDto() { NodeId = 123, Path = "-1,123" };
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Path.StartsWith(content.Path) && x.NodeId != content.NodeId, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[path]) LIKE upper(@0) AND ([umbracoNode].[id] <> @1)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
            Assert.AreEqual(content.NodeId, sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_Not_Starts_With()
        {
            const int level = 1;
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Level == level && x.Path.StartsWith("-20") == false, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(level, sql.Arguments[0]);
            Assert.AreEqual("-20%", sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_EqualsFalse_Starts_With()
        {
            const int level = 1;
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Level == level && x.Path.StartsWith("-20") == false, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(level, sql.Arguments[0]);
            Assert.AreEqual("-20%", sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_Equals_Clause()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Text.Equals("Hello@world.com"), _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) = upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("Hello@world.com", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_False_Boolean()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Trashed == false, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (NOT ([umbracoNode].[trashed] = @0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(true, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_EqualsFalse_Boolean()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Trashed == false, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (NOT ([umbracoNode].[trashed] = @0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(true, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Boolean()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Trashed, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(true, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_ToUpper()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Text.ToUpper() == "hello".ToUpper(), _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[text]) = upper(@0)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("hello", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_ToString()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Text == 1.ToString(), _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[text] = @0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("1", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Wildcard()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Text.StartsWith("D"), _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) LIKE upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("D%", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_Single_Constant()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.NodeId == 2, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] = @0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(2, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_And_Constant()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.NodeId != 2 && x.NodeId != 3, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[id] <> @0) AND ([umbracoNode].[id] <> @1)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(2, sql.Arguments[0]);
            Assert.AreEqual(3, sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_Or_Constant()
        {
            var sql = new Sql("SELECT *")
                .From<NodeDto>(_sqlSyntax)
                .Where<NodeDto>(x => x.Text == "hello" || x.NodeId == 3, _sqlSyntax);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[text] = @0) OR ([umbracoNode].[id] = @1)))", sql.SQL.Replace("\n", " "));
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
            sql.Select("*").From<ContentDto>(_sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
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
            sql.Select("*").From<DocumentDto>(_sqlSyntax)
                .InnerJoin<ContentVersionDto>(_sqlSyntax)
                .On<DocumentDto, ContentVersionDto>(_sqlSyntax, left => left.VersionId, right => right.VersionId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_OrderBy_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").OrderBy("([cmsContent].[contentType])");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>(_sqlSyntax).OrderBy<ContentDto>(x => x.ContentTypeId, _sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_GroupBy_With_Type()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").GroupBy("[contentType]");

            var sql = new Sql();
            sql.Select("*").From<ContentDto>(_sqlSyntax).GroupBy<ContentDto>(x => x.ContentTypeId, _sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_Predicate()
        {
            var expected = new Sql();
            expected.Select("*").From("[cmsContent]").Where("([cmsContent].[nodeId] = @0)", 1045);

            var sql = new Sql();
            sql.Select("*").From<ContentDto>(_sqlSyntax).Where<ContentDto>(x => x.NodeId == 1045, _sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_And_Predicate()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsContent]")
                .Where("([cmsContent].[nodeId] = @0)", 1045)
                .Where("([cmsContent].[contentType] = @0)", 1050);

            var sql = new Sql();
            sql.Select("*")
                .From<ContentDto>(_sqlSyntax)
                .Where<ContentDto>(x => x.NodeId == 1045, _sqlSyntax)
                .Where<ContentDto>(x => x.ContentTypeId == 1050, _sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Use_Select_With_Star_And_Predicate()
        {
            var expected = new Sql();
            expected.Select("[cmsContent].*")
                .From("[cmsContent]");

            var sql = new Sql();
            sql.Select<ContentDto>(_sqlSyntax)
                .From<ContentDto>(_sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Use_Select_With_One_Column_And_Predicate()
        {
            var expected = new Sql();
            expected.Select("[cmsContent].[nodeId]")
                .From("[cmsContent]");

            var sql = new Sql();
            sql.Select<ContentDto>(_sqlSyntax, c => c.NodeId)
                .From<ContentDto>(_sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Use_Select_With_Multiple_Column_And_Predicate()
        {
            var expected = new Sql();
            expected.Select("[cmsContent].[nodeId]", "[cmsContent].[contentType]", "[cmsContent].[pk]")
                .From("[cmsContent]");

            var sql = new Sql();
            sql.Select<ContentDto>(_sqlSyntax, c => c.NodeId, c => c.ContentTypeId, c => c.PrimaryKey)
                .From<ContentDto>(_sqlSyntax);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_InnerJoin_With_Select_And_AndSelect()
        {
            var expected = new Sql();
            expected.Select("[cmsDocument].[nodeId], [cmsDocument].[published]\n, [cmsContentVersion].[id]")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]")
                .On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]");

            var sql = new Sql();
            sql.Select<DocumentDto>(_sqlSyntax, d => d.NodeId, d => d.Published)
                .AndSelect<ContentVersionDto>(_sqlSyntax, cv => cv.Id)
                .From<DocumentDto>(_sqlSyntax)
                .InnerJoin<ContentVersionDto>(_sqlSyntax)
                .On<DocumentDto, ContentVersionDto>(_sqlSyntax, left => left.VersionId, right => right.VersionId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }
    }
}
