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

/// <summary>
/// Contains unit tests for NPoco SQL functionality within Umbraco's persistence infrastructure.
/// </summary>
[TestFixture]
public class NPocoSqlTests : BaseUsingSqlSyntax
{
    /// <summary>
    /// Tests the SQL generation for a WHERE clause using the SqlStartsWith method with additional parameters.
    /// </summary>
    [Test]
    public void Where_Clause_With_Starts_With_Additional_Parameters()
    {
        var content = new NodeDto { NodeId = 123, Path = "-1,123" };
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Path.SqlStartsWith(content.Path, TextColumnType.NVarchar));

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[path]) LIKE upper(@0))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that a WHERE clause with a StartsWith condition using a variable is correctly translated to SQL.
    /// </summary>
    [Test]
    public void Where_Clause_With_Starts_With_By_Variable()
    {
        var content = new NodeDto { NodeId = 123, Path = "-1,123" };
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Path.StartsWith(content.Path) && x.NodeId != content.NodeId);

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[path]) LIKE upper(@0) AND ([umbracoNode].[id] <> @1)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
        Assert.AreEqual(content.NodeId, sql.Arguments[1]);
    }

    /// <summary>
    /// Tests that a WHERE clause with a NOT and StartsWith condition is correctly translated to SQL.
    /// </summary>
    [Test]
    public void Where_Clause_With_Not_Starts_With()
    {
        const int level = 1;
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Level == level && !x.Path.StartsWith("-20"));

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(level, sql.Arguments[0]);
        Assert.AreEqual("-20%", sql.Arguments[1]);
    }

    /// <summary>
    /// Tests that a WHERE clause using <c>StartsWith</c> with <c>equals false</c> is correctly translated to SQL, ensuring the resulting SQL uses <c>NOT LIKE</c> for string matching.
    /// </summary>
    [Test]
    public void Where_Clause_With_EqualsFalse_Starts_With()
    {
        const int level = 1;
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Level == level && x.Path.StartsWith("-20") == false);

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[level] = @0) AND NOT (upper([umbracoNode].[path]) LIKE upper(@1))))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(level, sql.Arguments[0]);
        Assert.AreEqual("-20%", sql.Arguments[1]);
    }

    /// <summary>
    /// Tests that a WHERE clause with an Equals condition is correctly generated in the SQL statement.
    /// </summary>
    [Test]
    public void Where_Clause_With_Equals_Clause()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.Equals("Hello@world.com"));

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) = upper(@0))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("Hello@world.com", sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that a WHERE clause with a false boolean condition is correctly generated.
    /// </summary>
    [Test]
    public void Where_Clause_With_False_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Trashed == false);

        Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(false, sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that a WHERE clause with a boolean property compared to false is correctly generated.
    /// </summary>
    [Test]
    public void Where_Clause_With_EqualsFalse_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>().Where<NodeDto>(x => x.Trashed == false);

        Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(false, sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that the WHERE clause correctly handles boolean values.
    /// </summary>
    [Test]
    public void Where_Clause_With_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Trashed);

        Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE ([umbracoNode].[trashed] = @0)", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(true, sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that the SQL generated for a WHERE clause with a ToUpper() call
    /// on a property correctly translates to SQL with upper() function calls.
    /// </summary>
    [Test]
    public void Where_Clause_With_ToUpper()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.ToUpper() == "hello".ToUpper());

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE ((upper([umbracoNode].[text]) = upper(@0)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("hello", sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that a WHERE clause using ToString() in the expression is correctly translated to SQL.
    /// </summary>
    [Test]
    public void Where_Clause_With_ToString()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text == 1.ToString());

        Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[text] = @0))", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("1", sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that a WHERE clause with a wildcard is correctly generated using StartsWith.
    /// </summary>
    [Test]
    public void Where_Clause_With_Wildcard()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.StartsWith("D"));

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE (upper([umbracoNode].[text]) LIKE upper(@0))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("D%", sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that a SQL WHERE clause with a single constant condition is generated correctly.
    /// </summary>
    [Test]
    public void Where_Clause_Single_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId == 2);

        Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] = @0))", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(2, sql.Arguments[0]);
    }

    /// <summary>
    /// Tests that the SQL generated by NPoco correctly handles a WHERE clause with AND and constant values.
    /// </summary>
    [Test]
    public void Where_Clause_And_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId != 2 && x.NodeId != 3);

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[id] <> @0) AND ([umbracoNode].[id] <> @1)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(2, sql.Arguments[0]);
        Assert.AreEqual(3, sql.Arguments[1]);
    }

    /// <summary>
    /// Tests the SQL generation for a WHERE clause combining a property comparison and a constant using OR.
    /// </summary>
    [Test]
    public void Where_Clause_Or_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text == "hello" || x.NodeId == 3);

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE ((([umbracoNode].[text] = @0) OR ([umbracoNode].[id] = @1)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual("hello", sql.Arguments[0]);
        Assert.AreEqual(3, sql.Arguments[1]);
    }

    /// <summary>
    /// Tests that the SQL generated by WhereNull correctly handles null conditions.
    /// </summary>
    [Test]
    public void Where_Null()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereNull<NodeDto>(x => x.NodeId);
        Assert.AreEqual("SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] IS NULL))", sql.SQL.Replace("\n", " "));
    }

    /// <summary>
    /// Tests that the generated SQL query correctly includes a WHERE NOT NULL clause.
    /// </summary>
    [Test]
    public void Where_Not_Null()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereNotNull<NodeDto>(x => x.NodeId);
        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE (([umbracoNode].[id] IS NOT NULL))",
            sql.SQL.Replace("\n", " "));
    }

    /// <summary>
    /// Tests the SQL generation for the WhereAny method with multiple OR conditions.
    /// </summary>
    [Test]
    public void Where_Any()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereAny(
            s => s.Where<NodeDto>(x => x.NodeId == 1),
            s => s.Where<NodeDto>(x => x.NodeId == 2));

        Assert.AreEqual(
            "SELECT * FROM [umbracoNode] WHERE (( (([umbracoNode].[id] = @0)) ) OR ( (([umbracoNode].[id] = @1)) ))",
            sql.SQL.Replace("\n", " "));
    }

    /// <summary>
    /// Tests that the SQL generated by selecting all columns from a table using a type parameter
    /// matches the SQL generated by selecting all columns from the table name directly.
    /// </summary>
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

    /// <summary>
    /// Tests that an inner join can be correctly constructed using NPoco with typed DTOs.
    /// </summary>
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

    /// <summary>
    /// Verifies that NPoco SQL generation correctly handles ordering by a property using its type-safe expression syntax.
    /// Ensures that the generated SQL matches the expected SQL when ordering by a specific property (e.g., ContentTypeId).
    /// </summary>
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

    /// <summary>
    /// Tests that the SQL generated by grouping by a type property matches the expected SQL.
    /// </summary>
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

    /// <summary>
    /// Tests that a WHERE predicate can be used correctly in NPoco SQL queries.
    /// </summary>
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

    /// <summary>
    /// Tests that the SQL builder can correctly use multiple Where predicates combined with AND.
    /// </summary>
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

    /// <summary>
    /// Tests the SQL generation for the ForUpdate method, ensuring the correct SQL syntax with UPDLOCK is produced.
    /// </summary>
    [Test]
    public void ForUpdate()
    {
        var sessionId = Guid.NewGuid();

        var sql = Sql()
            .SelectAll()
            .From<UserLoginDto>()
            .Where<UserLoginDto>(x => x.SessionId == sessionId);

        Assert.AreEqual(
            "SELECT * FROM [umbracoUserLogin] WHERE (([umbracoUserLogin].[sessionId] = @0))",
            sql.SQL.NoCrLf());

        sql = sql.ForUpdate();

        Assert.AreEqual(
            "SELECT * FROM [umbracoUserLogin] WITH (UPDLOCK) WHERE (([umbracoUserLogin].[sessionId] = @0))",
            sql.SQL.NoCrLf());
    }
}
