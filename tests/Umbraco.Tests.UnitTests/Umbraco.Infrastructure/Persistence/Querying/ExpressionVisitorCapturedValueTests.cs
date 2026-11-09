// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using System.Runtime;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

/// <summary>
///     Tests that the SQL expression visitors read values captured by a query expression (locals, members of captured
///     objects, statics and computed values) into SQL parameters.
/// </summary>
[TestFixture]
public class ExpressionVisitorCapturedValueTests : BaseUsingSqlSyntax
{
    private static int _staticParentId = -20;

    [Test]
    public void Model_Visitor_Reads_Captured_Local()
    {
        var parentId = 1234;

        var (sql, parameters) = VisitModel<IContent>(x => x.ParentId == parentId);

        Assert.AreEqual("([umbracoNode].[parentId] = @0)", sql);
        CollectionAssert.AreEqual(new object[] { 1234 }, parameters);
    }

    [Test]
    public void Model_Visitor_Reads_Member_Of_Captured_Object()
    {
        var filter = new Filter { Id = 99, Inner = new Filter { Name = "inner" } };

        var (sql, parameters) = VisitModel<IContent>(x => x.ParentId == filter.Id && x.Name == filter.Inner.Name);

        Assert.AreEqual("(([umbracoNode].[parentId] = @0) AND ([umbracoContentVersion].[text] = @1))", sql);
        CollectionAssert.AreEqual(new object[] { 99, "inner" }, parameters);
    }

    [Test]
    public void Model_Visitor_Reads_Static_Member()
    {
        var (sql, parameters) = VisitModel<IContent>(x => x.ParentId == _staticParentId);

        Assert.AreEqual("([umbracoNode].[parentId] = @0)", sql);
        CollectionAssert.AreEqual(new object[] { -20 }, parameters);
    }

    [Test]
    public void Model_Visitor_Reads_Captured_Nullable()
    {
        int? creatorId = 5;

        var (sql, parameters) = VisitModel<IContent>(x => x.CreatorId == creatorId);

        Assert.AreEqual("([umbracoNode].[nodeUser] = @0)", sql);
        CollectionAssert.AreEqual(new object[] { 5 }, parameters);
    }

    [TestCase("StartsWith", "upper([umbracoNode].[path]) LIKE upper(@0)", "-1,123%")]
    [TestCase("EndsWith", "upper([umbracoNode].[path]) LIKE upper(@0)", "%-1,123")]
    [TestCase("Contains", "upper([umbracoNode].[path]) LIKE upper(@0)", "%-1,123%")]
    [TestCase("Equals", "upper([umbracoNode].[path]) = upper(@0)", "-1,123")]
    [TestCase("InvariantStartsWith", "upper([umbracoNode].[path]) LIKE upper(@0)", "-1,123%")]
    [TestCase("InvariantEndsWith", "upper([umbracoNode].[path]) LIKE upper(@0)", "%-1,123")]
    [TestCase("InvariantContains", "upper([umbracoNode].[path]) LIKE upper(@0)", "%-1,123%")]
    [TestCase("InvariantEquals", "upper([umbracoNode].[path]) = upper(@0)", "-1,123")]
    [TestCase("SqlStartsWith", "upper([umbracoNode].[path]) LIKE upper(@0)", "-1,123%")]
    [TestCase("SqlEndsWith", "upper([umbracoNode].[path]) LIKE upper(@0)", "%-1,123")]
    [TestCase("SqlContains", "upper([umbracoNode].[path]) LIKE upper(@0)", "%-1,123%")]
    [TestCase("SqlEquals", "upper([umbracoNode].[path]) = upper(@0)", "-1,123")]
    public void Model_Visitor_Reads_Captured_String_Comparison_Argument(string method, string expectedSql, string expectedParameter)
    {
        var parent = new Filter { Name = "-1,123" };

        Expression<Func<IContent, bool>> predicate = method switch
        {
            "StartsWith" => x => x.Path.StartsWith(parent.Name!),
            "EndsWith" => x => x.Path.EndsWith(parent.Name!),
            "Contains" => x => x.Path.Contains(parent.Name!),
            "Equals" => x => x.Path.Equals(parent.Name),
            "InvariantStartsWith" => x => x.Path.InvariantStartsWith(parent.Name!),
            "InvariantEndsWith" => x => x.Path.InvariantEndsWith(parent.Name!),
            "InvariantContains" => x => x.Path.InvariantContains(parent.Name!),
            "InvariantEquals" => x => x.Path.InvariantEquals(parent.Name),
            "SqlStartsWith" => x => x.Path.SqlStartsWith(parent.Name!, TextColumnType.NVarchar),
            "SqlEndsWith" => x => x.Path.SqlEndsWith(parent.Name!, TextColumnType.NVarchar),
            "SqlContains" => x => x.Path.SqlContains(parent.Name!, TextColumnType.NVarchar),
            "SqlEquals" => x => x.Path.SqlEquals(parent.Name!, TextColumnType.NVarchar),
            _ => throw new ArgumentOutOfRangeException(nameof(method)),
        };

        var (sql, parameters) = VisitModel(predicate);

        Assert.AreEqual(expectedSql, sql);
        CollectionAssert.AreEqual(new object[] { expectedParameter }, parameters);
    }

    [Test]
    public void Model_Visitor_Reads_Computed_String_Comparison_Argument()
    {
        var parent = new Filter { Name = "-1,123" };

        var (sql, parameters) = VisitModel<IContent>(x => x.Path.StartsWith(parent.Name + ","));

        Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", sql);
        CollectionAssert.AreEqual(new object[] { "-1,123,%" }, parameters);
    }

    [Test]
    public void Model_Visitor_Reads_Captured_Replace_Arguments()
    {
        var search = "@world";
        var replacement = new Filter { Name = "@test" };

        var (sql, parameters) = VisitModel<IUser>(x => x.Username.Replace(search, replacement.Name) == "hello@test.com");

        Assert.AreEqual("(replace([umbracoUser].[userLogin], @1, @2) = @0)", sql);
        CollectionAssert.AreEqual(new object[] { "hello@test.com", "@world", "@test" }, parameters);
    }

    [Test]
    public void Model_Visitor_Reads_Captured_Collection_For_In_Clause()
    {
        var filter = new Filter { Names = ["a@b.com", "c@d.com"] };

        var (sql, parameters) = VisitModel<IUser>(x => filter.Names.Contains(x.Username));

        Assert.AreEqual("[umbracoUser].[userLogin] IN (@1,@2)", sql);
        Assert.AreEqual("a@b.com", parameters[1]);
        Assert.AreEqual("c@d.com", parameters[2]);
    }

    [Test]
    public void Model_Visitor_Writes_Null_For_Empty_Captured_Collection_In_Clause()
    {
        var names = new List<string>();

        var (sql, _) = VisitModel<IUser>(x => names.Contains(x.Username));

        Assert.AreEqual("[umbracoUser].[userLogin] IN (NULL)", sql);
    }

    [Test]
    public void Poco_Visitor_Reads_Captured_Values()
    {
        var nodeId = 1234;
        var filter = new Filter { Name = "-1,1234" };

        var (sql, parameters) = VisitPoco<NodeDto>(x => x.NodeId == nodeId && x.Path == filter.Name && x.ParentId == _staticParentId);

        Assert.AreEqual(
            "((([umbracoNode].[id] = @0) AND ([umbracoNode].[path] = @1)) AND ([umbracoNode].[parentId] = @2))",
            sql);
        CollectionAssert.AreEqual(new object[] { 1234, "-1,1234", -20 }, parameters);
    }

    [Test]
    public void Poco_Visitor_Reads_Captured_String_Comparison_Argument()
    {
        var filter = new Filter { Name = "-1,1234" };

        var (sql, parameters) = VisitPoco<NodeDto>(x => x.Path.StartsWith(filter.Name!));

        Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", sql);
        CollectionAssert.AreEqual(new object[] { "-1,1234%" }, parameters);
    }

    [Test]
    public void Poco_Visitor_Reads_New_Object_Value()
    {
        var (sql, parameters) = VisitPoco<NodeDto>(x => x.CreateDate == new DateTime(2020, 1, 2));

        Assert.AreEqual("([umbracoNode].[createDate] = @0)", sql);
        CollectionAssert.AreEqual(new object[] { new DateTime(2020, 1, 2) }, parameters);
    }

    [Test]
    public void Poco_Visitor_Lists_Columns_Of_New_Object_Using_The_Lambda_Parameter()
    {
        Expression<Func<NodeDto, object>> selector = x => new { x.NodeId, x.Path };
        var visitor = new PocoToSqlExpressionVisitor<NodeDto>(SqlContext, null);

        var sql = visitor.Visit(selector);

        Assert.AreEqual("[umbracoNode].[id],[umbracoNode].[path]", sql);
        CollectionAssert.IsEmpty(visitor.GetSqlParameters());
    }

    [Test]
    public void Poco_Visitor_Reads_Captured_Template_Argument_Name()
    {
        var argumentName = "value";

        var (sql, parameters) = VisitPoco<NodeDto>(x => x.Path == SqlTemplate.Arg<string>(argumentName));

        Assert.AreEqual("([umbracoNode].[path] = @0)", sql);
        Assert.AreEqual(1, parameters.Length);
        Assert.IsInstanceOf<SqlTemplate.TemplateArg>(parameters[0]);
        Assert.AreEqual("value", ((SqlTemplate.TemplateArg)parameters[0]).Name);
    }

    [Test]
    public void Two_Poco_Visitor_Reads_Captured_Values()
    {
        var nodeId = 1234;
        var filter = new Filter { Name = "editors" };
        Expression<Func<NodeDto, UserGroupDto, bool>> predicate = (node, group) => node.NodeId == nodeId && group.Name == filter.Name;
        var visitor = new PocoToSqlExpressionVisitor<NodeDto, UserGroupDto>(SqlContext, null, null);

        var sql = visitor.Visit(predicate);

        Assert.AreEqual("(([umbracoNode].[id] = @0) AND ([umbracoUserGroup].[userGroupName] = @1))", sql);
        CollectionAssert.AreEqual(new object[] { 1234, "editors" }, visitor.GetSqlParameters());
    }

    [Test]
    public void Three_Poco_Visitor_Reads_Captured_Values()
    {
        var nodeId = 1234;
        var filter = new Filter { Name = "editors", Id = 5 };
        Expression<Func<NodeDto, UserGroupDto, UserDto, bool>> predicate = (node, group, user) =>
            node.NodeId == nodeId && group.Name == filter.Name && user.Id == filter.Id;
        var visitor = new PocoToSqlExpressionVisitor<NodeDto, UserGroupDto, UserDto>(SqlContext, null, null, null);

        var sql = visitor.Visit(predicate);

        Assert.AreEqual(
            "((([umbracoNode].[id] = @0) AND ([umbracoUserGroup].[userGroupName] = @1)) AND ([umbracoUser].[id] = @2))",
            sql);
        CollectionAssert.AreEqual(new object[] { 1234, "editors", 5 }, visitor.GetSqlParameters());
    }

    [Test]
    public void Poco_Visitor_Applies_Sql_Text_With_One_Field()
    {
        Expression<Func<NodeDto, object>> predicate = node =>
            SqlExtensionsStatics.SqlText<bool>(node.Path, n1 => $"({n1} LIKE 'x')");
        var visitor = new PocoToSqlExpressionVisitor<NodeDto>(SqlContext, null);

        Assert.AreEqual("([umbracoNode].[path] LIKE 'x')", visitor.Visit(predicate));
    }

    [Test]
    public void Two_Poco_Visitor_Applies_Sql_Text_With_Two_Fields()
    {
        Expression<Func<UserDto, UserGroupDto, object>> predicate = (user, group) =>
            SqlExtensionsStatics.SqlText<bool>(user.Login, group.Name, (n1, n2) => $"({n1} LIKE concat({n2}, ',%'))");
        var visitor = new PocoToSqlExpressionVisitor<UserDto, UserGroupDto>(SqlContext, null, null);

        Assert.AreEqual(
            "([umbracoUser].[userLogin] LIKE concat([umbracoUserGroup].[userGroupName], ',%'))",
            visitor.Visit(predicate));
    }

    [Test]
    public void Three_Poco_Visitor_Applies_Sql_Text_With_Three_Fields()
    {
        Expression<Func<NodeDto, UserGroupDto, UserDto, object>> predicate = (node, group, user) =>
            SqlExtensionsStatics.SqlText<bool>(node.Path, group.Name, user.Login, (n1, n2, n3) => $"({n1} = {n2} OR {n1} = {n3})");
        var visitor = new PocoToSqlExpressionVisitor<NodeDto, UserGroupDto, UserDto>(SqlContext, null, null, null);

        Assert.AreEqual(
            "([umbracoNode].[path] = [umbracoUserGroup].[userGroupName] OR [umbracoNode].[path] = [umbracoUser].[userLogin])",
            visitor.Visit(predicate));
    }

    [Test]
    public void Model_Visitor_Does_Not_Compile_Code_Per_Captured_Value()
        => AssertDoesNotCompileCodePerVisit(i =>
        {
            var parent = new Filter { Name = "-1," + i };
            VisitModel<IContent>(x => x.ParentId == i && x.Path.StartsWith(parent.Name!));
        });

    [Test]
    public void Poco_Visitor_Does_Not_Compile_Code_Per_Captured_Value()
        => AssertDoesNotCompileCodePerVisit(i =>
        {
            VisitPoco<NodeDto>(x => x.NodeId == i);
        });

    private static void AssertDoesNotCompileCodePerVisit(Action<int> visit)
    {
        const int iterations = 200;

        visit(0);
        var compiledBefore = JitInfo.GetCompiledMethodCount(currentThread: true);

        for (var i = 0; i < iterations; i++)
        {
            visit(i);
        }

        var compiled = JitInfo.GetCompiledMethodCount(currentThread: true) - compiledBefore;
        Assert.Less(compiled, iterations, $"{compiled} methods were JIT-compiled over {iterations} visits.");
    }

    private (string Sql, object[] Parameters) VisitModel<T>(Expression<Func<T, bool>> predicate)
    {
        var visitor = new ModelToSqlExpressionVisitor<T>(SqlContext.SqlSyntax, Mappers);
        var sql = visitor.Visit(predicate);
        return (sql, visitor.GetSqlParameters());
    }

    private (string Sql, object[] Parameters) VisitPoco<TDto>(Expression<Func<TDto, bool>> predicate)
    {
        var visitor = new PocoToSqlExpressionVisitor<TDto>(SqlContext, null);
        var sql = visitor.Visit(predicate);
        return (sql, visitor.GetSqlParameters());
    }

    private class Filter
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public Filter Inner { get; set; } = null!;

        public List<string> Names { get; set; } = [];
    }
}
