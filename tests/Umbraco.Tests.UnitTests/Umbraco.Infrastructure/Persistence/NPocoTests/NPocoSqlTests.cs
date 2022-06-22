// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.NPocoTests
{
    [TestFixture]
    public class NPocoSqlTests : BaseUsingSqlSyntax
    {
        [Test]
        public void Where_Clause_With_Starts_With_Additional_Parameters()
        {
            var content = new NodeDto() { NodeId = 123, Path = "-1,123" };
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Path.SqlStartsWith(content.Path, TextColumnType.NVarchar));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[path]) LIKE upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Starts_With_By_Variable()
        {
            var content = new NodeDto() { NodeId = 123, Path = "-1,123" };
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Path.StartsWith(content.Path) && x.NodeId != content.NodeId);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[path]) LIKE upper(@0) AND ([umbracoNode].[id] <> @1)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
            Assert.AreEqual(content.NodeId, sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_Not_Starts_With()
        {
            const int level = 1;
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Level == level && !x.Path.StartsWith("-20"));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(level, sql.Arguments[0]);
            Assert.AreEqual("-20%", sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_EqualsFalse_Starts_With()
        {
            const int level = 1;
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Level == level && x.Path.StartsWith("-20") == false);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(level, sql.Arguments[0]);
            Assert.AreEqual("-20%", sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_With_Equals_Clause()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Text.Equals("Hello@world.com"));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) = upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("Hello@world.com", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_False_Boolean()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Trashed == false);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(false, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_EqualsFalse_Boolean()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>().Where<NodeDto>(x => x.Trashed == false);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(false, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Boolean()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Trashed);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(true, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_ToUpper()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Text.ToUpper() == "hello".ToUpper());

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[text]) = upper(@0)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("hello", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_ToString()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Text == 1.ToString());

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[text] = @0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("1", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_With_Wildcard()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Text.StartsWith("D"));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) LIKE upper(@0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual("D%", sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_Single_Constant()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.NodeId == 2);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] = @0))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(1, sql.Arguments.Length);
            Assert.AreEqual(2, sql.Arguments[0]);
        }

        [Test]
        public void Where_Clause_And_Constant()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.NodeId != 2 && x.NodeId != 3);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[id] <> @0) AND ([umbracoNode].[id] <> @1)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual(2, sql.Arguments[0]);
            Assert.AreEqual(3, sql.Arguments[1]);
        }

        [Test]
        public void Where_Clause_Or_Constant()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>()
                .Where<NodeDto>(x => x.Text == "hello" || x.NodeId == 3);

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[text] = @0) OR ([umbracoNode].[id] = @1)))", sql.SQL.Replace("\n", " "));
            Assert.AreEqual(2, sql.Arguments.Length);
            Assert.AreEqual("hello", sql.Arguments[0]);
            Assert.AreEqual(3, sql.Arguments[1]);
        }

        [Test]
        public void Where_Null()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>().WhereNull<NodeDto>(x => x.NodeId);
            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] IS NULL))", sql.SQL.Replace("\n", " "));
        }

        [Test]
        public void Where_Not_Null()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>().WhereNotNull<NodeDto>(x => x.NodeId);
            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] IS NOT NULL))", sql.SQL.Replace("\n", " "));
        }

        [Test]
        public void Where_Any()
        {
            Sql<ISqlContext> sql = Sql().SelectAll().From<NodeDto>().WhereAny(
                s => s.Where<NodeDto>(x => x.NodeId == 1),
                s => s.Where<NodeDto>(x => x.NodeId == 2));

            Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (( (([umbracoNode].[id] = @0)) ) OR ( (([umbracoNode].[id] = @1)) ))", sql.SQL.Replace("\n", " "));
        }

        [Test]
        public void Can_Select_From_With_Type()
        {
            Sql<ISqlContext> expected = Sql();
            expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]");

            Sql<ISqlContext> sql = Sql();
            sql.SelectAll().From<ContentDto>();

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_InnerJoin_With_Types()
        {
            Sql<ISqlContext> expected = Sql();
            expected.SelectAll()
                .From($"[{Constants.DatabaseSchema.Tables.DocumentVersion}]")
                .InnerJoin($"[{Constants.DatabaseSchema.Tables.ContentVersion}]")
                .On($"[{Constants.DatabaseSchema.Tables.DocumentVersion}].[id] = [{Constants.DatabaseSchema.Tables.ContentVersion}].[id]");

            Sql<ISqlContext> sql = Sql();
            sql.SelectAll().From<DocumentVersionDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentVersionDto, ContentVersionDto>(left => left.Id, right => right.Id);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_OrderBy_With_Type()
        {
            Sql<ISqlContext> expected = Sql();
            expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]").OrderBy($"([{Constants.DatabaseSchema.Tables.Content}].[contentTypeId])");

            Sql<ISqlContext> sql = Sql();
            sql.SelectAll().From<ContentDto>().OrderBy<ContentDto>(x => x.ContentTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_GroupBy_With_Type()
        {
            Sql<ISqlContext> expected = Sql();
            expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]").GroupBy($"[{Constants.DatabaseSchema.Tables.Content}].[contentTypeId]");

            Sql<ISqlContext> sql = Sql();
            sql.SelectAll().From<ContentDto>().GroupBy<ContentDto>(x => x.ContentTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_Predicate()
        {
            Sql<ISqlContext> expected = Sql();
            expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]").Where($"([{Constants.DatabaseSchema.Tables.Content}].[nodeId] = @0)", 1045);

            Sql<ISqlContext> sql = Sql();
            sql.SelectAll().From<ContentDto>().Where<ContentDto>(x => x.NodeId == 1045);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void Can_Use_Where_And_Predicate()
        {
            Sql<ISqlContext> expected = Sql();
            expected.SelectAll()
                .From($"[{Constants.DatabaseSchema.Tables.Content}]")
                .Where($"([{Constants.DatabaseSchema.Tables.Content}].[nodeId] = @0)", 1045)
                .Where($"([{Constants.DatabaseSchema.Tables.Content}].[contentTypeId] = @0)", 1050);

            Sql<ISqlContext> sql = Sql();
            sql.SelectAll()
                .From<ContentDto>()
                .Where<ContentDto>(x => x.NodeId == 1045)
                .Where<ContentDto>(x => x.ContentTypeId == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Debug.Print(sql.SQL);
        }

        [Test]
        public void ForUpdate()
        {
            var sessionId = Guid.NewGuid();

            Sql<ISqlContext> sql = Sql()
                .SelectAll()
                .From<UserLoginDto>()
                .Where<UserLoginDto>(x => x.SessionId == sessionId);

            Assert.AreEqual("SELECT * FROM [umbracoUserLogin] WHERE (([umbracoUserLogin].[sessionId] = @0))", sql.SQL.NoCrLf());

            sql = sql.ForUpdate();

            Assert.AreEqual("SELECT * FROM [umbracoUserLogin] WITH (UPDLOCK) WHERE (([umbracoUserLogin].[sessionId] = @0))", sql.SQL.NoCrLf());
        }
    }
}
