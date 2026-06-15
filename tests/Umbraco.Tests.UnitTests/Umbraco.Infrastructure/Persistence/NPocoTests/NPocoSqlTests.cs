// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
public class NPocoSqlTests : BaseUsingSqlSyntax
{
    [Test]
    public void Where_Clause_With_Starts_With_Additional_Parameters()
    {
        var content = new NodeDto { NodeId = 123, Path = "-1,123" };
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Path.SqlStartsWith(content.Path, TextColumnType.NVarchar));

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[path]) LIKE upper(@0))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo(content.Path + "%"));
    }

    [Test]
    public void Where_Clause_With_Starts_With_By_Variable()
    {
        var content = new NodeDto { NodeId = 123, Path = "-1,123" };
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Path.StartsWith(content.Path) && x.NodeId != content.NodeId);

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[path]) LIKE upper(@0) AND ([umbracoNode].[id] <> @1)))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(2));
        Assert.That(sql.Arguments[0], Is.EqualTo(content.Path + "%"));
        Assert.That(sql.Arguments[1], Is.EqualTo(content.NodeId));
    }

    [Test]
    public void Where_Clause_With_Not_Starts_With()
    {
        const int level = 1;
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Level == level && !x.Path.StartsWith("-20"));

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(2));
        Assert.That(sql.Arguments[0], Is.EqualTo(level));
        Assert.That(sql.Arguments[1], Is.EqualTo("-20%"));
    }

    [Test]
    public void Where_Clause_With_EqualsFalse_Starts_With()
    {
        const int level = 1;
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Level == level && x.Path.StartsWith("-20") == false);

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(2));
        Assert.That(sql.Arguments[0], Is.EqualTo(level));
        Assert.That(sql.Arguments[1], Is.EqualTo("-20%"));
    }

    [Test]
    public void Where_Clause_With_Equals_Clause()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.Equals("Hello@world.com"));

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) = upper(@0))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo("Hello@world.com"));
    }

    [Test]
    public void Where_Clause_With_False_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Trashed == false);

        Assert.That(sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo(false));
    }

    [Test]
    public void Where_Clause_With_EqualsFalse_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>().Where<NodeDto>(x => x.Trashed == false);

        Assert.That(sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo(false));
    }

    [Test]
    public void Where_Clause_With_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Trashed);

        Assert.That(sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo(true));
    }

    [Test]
    public void Where_Clause_With_ToUpper()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.ToUpper() == "hello".ToUpper());

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[text]) = upper(@0)))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo("hello"));
    }

    [Test]
    public void Where_Clause_With_ToString()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text == 1.ToString());

        Assert.That(sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[text] = @0))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo("1"));
    }

    [Test]
    public void Where_Clause_With_Wildcard()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.StartsWith("D"));

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) LIKE upper(@0))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo("D%"));
    }

    [Test]
    public void Where_Clause_Single_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId == 2);

        Assert.That(sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] = @0))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo(2));
    }

    [Test]
    public void Where_Clause_And_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId != 2 && x.NodeId != 3);

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[id] <> @0) AND ([umbracoNode].[id] <> @1)))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(2));
        Assert.That(sql.Arguments[0], Is.EqualTo(2));
        Assert.That(sql.Arguments[1], Is.EqualTo(3));
    }

    [Test]
    public void Where_Clause_Or_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text == "hello" || x.NodeId == 3);

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[text] = @0) OR ([umbracoNode].[id] = @1)))"));
        Assert.That(sql.Arguments.Length, Is.EqualTo(2));
        Assert.That(sql.Arguments[0], Is.EqualTo("hello"));
        Assert.That(sql.Arguments[1], Is.EqualTo(3));
    }

    [Test]
    public void Where_Null()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereNull<NodeDto>(x => x.NodeId);
        Assert.That(sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] IS NULL))"));
    }

    [Test]
    public void Where_Not_Null()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereNotNull<NodeDto>(x => x.NodeId);
        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] IS NOT NULL))"));
    }

    [Test]
    public void Where_Any()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereAny(
            s => s.Where<NodeDto>(x => x.NodeId == 1),
            s => s.Where<NodeDto>(x => x.NodeId == 2));

        Assert.That(
            sql.SQL.Replace("\n", " "), Is.EqualTo("SELECT * FROM [umbracoNode] WHERE (( (([umbracoNode].[id] = @0)) ) OR ( (([umbracoNode].[id] = @1)) ))"));
    }

    [Test]
    public void Can_Select_From_With_Type()
    {
        var expected = Sql();
        expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]");

        var sql = Sql();
        sql.SelectAll().From<ContentDto>();

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_InnerJoin_With_Types()
    {
        var expected = Sql();
        expected.SelectAll()
            .From($"[{Constants.DatabaseSchema.Tables.DocumentVersion}]")
            .InnerJoin($"[{Constants.DatabaseSchema.Tables.ContentVersion}]")
            .On(
                $"[{Constants.DatabaseSchema.Tables.DocumentVersion}].[id] = [{Constants.DatabaseSchema.Tables.ContentVersion}].[id]");

        var sql = Sql();
        sql.SelectAll().From<DocumentVersionDto>()
            .InnerJoin<ContentVersionDto>()
            .On<DocumentVersionDto, ContentVersionDto>(left => left.Id, right => right.Id);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_OrderBy_With_Type()
    {
        var expected = Sql();
        expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]")
            .OrderBy($"([{Constants.DatabaseSchema.Tables.Content}].[contentTypeId])");

        var sql = Sql();
        sql.SelectAll().From<ContentDto>().OrderBy<ContentDto>(x => x.ContentTypeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_GroupBy_With_Type()
    {
        var expected = Sql();
        expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]")
            .GroupBy($"[{Constants.DatabaseSchema.Tables.Content}].[contentTypeId]");

        var sql = Sql();
        sql.SelectAll().From<ContentDto>().GroupBy<ContentDto>(x => x.ContentTypeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Use_Where_Predicate()
    {
        var expected = Sql();
        expected.SelectAll().From($"[{Constants.DatabaseSchema.Tables.Content}]")
            .Where($"([{Constants.DatabaseSchema.Tables.Content}].[nodeId] = @0)", 1045);

        var sql = Sql();
        sql.SelectAll().From<ContentDto>().Where<ContentDto>(x => x.NodeId == 1045);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Use_Where_And_Predicate()
    {
        var expected = Sql();
        expected.SelectAll()
            .From($"[{Constants.DatabaseSchema.Tables.Content}]")
            .Where($"([{Constants.DatabaseSchema.Tables.Content}].[nodeId] = @0)", 1045)
            .Where($"([{Constants.DatabaseSchema.Tables.Content}].[contentTypeId] = @0)", 1050);

        var sql = Sql();
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

        var sql = Sql()
            .SelectAll()
            .From<UserLoginDto>()
            .Where<UserLoginDto>(x => x.SessionId == sessionId);

        Assert.That(
            sql.SQL.NoCrLf(), Is.EqualTo("SELECT * FROM [umbracoUserLogin] WHERE (([umbracoUserLogin].[sessionId] = @0))"));

        sql = sql.ForUpdate();

        Assert.That(
            sql.SQL.NoCrLf(), Is.EqualTo("SELECT * FROM [umbracoUserLogin] WITH (UPDLOCK) WHERE (([umbracoUserLogin].[sessionId] = @0))"));
    }
}
