// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

/// <summary>
///     Tests for the one, two and three DTO <see cref="PocoToSqlExpressionVisitor{TDto}" /> variants, including the
///     <c>new</c> and <c>SqlText</c> handling they inherit from <see cref="ExpressionVisitorBase" />.
/// </summary>
[TestFixture]
public class PocoToSqlExpressionVisitorTests : BaseUsingSqlSyntax
{
    private static int _staticParentId = -20;

    [Test]
    public void Reads_Captured_Values()
    {
        var nodeId = 1234;
        var filter = new Filter { Name = "-1,1234" };

        var (sql, parameters) = Visit<NodeDto>(x => x.NodeId == nodeId && x.Path == filter.Name && x.ParentId == _staticParentId);

        Assert.AreEqual(
            "((([umbracoNode].[id] = @0) AND ([umbracoNode].[path] = @1)) AND ([umbracoNode].[parentId] = @2))",
            sql);
        CollectionAssert.AreEqual(new object[] { 1234, "-1,1234", -20 }, parameters);
    }

    [Test]
    public void Reads_Captured_String_Comparison_Argument()
    {
        var filter = new Filter { Name = "-1,1234" };

        var (sql, parameters) = Visit<NodeDto>(x => x.Path.StartsWith(filter.Name!));

        Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", sql);
        CollectionAssert.AreEqual(new object[] { "-1,1234%" }, parameters);
    }

    [Test]
    public void Reads_New_Object_Value()
    {
        var (sql, parameters) = Visit<NodeDto>(x => x.CreateDate == new DateTime(2020, 1, 2));

        Assert.AreEqual("([umbracoNode].[createDate] = @0)", sql);
        CollectionAssert.AreEqual(new object[] { new DateTime(2020, 1, 2) }, parameters);
    }

    [Test]
    public void Lists_Columns_Of_New_Object_Using_The_Lambda_Parameter()
    {
        Expression<Func<NodeDto, object>> selector = x => new { x.NodeId, x.Path };
        var visitor = new PocoToSqlExpressionVisitor<NodeDto>(SqlContext, null);

        var sql = visitor.Visit(selector);

        Assert.AreEqual("[umbracoNode].[id],[umbracoNode].[path]", sql);
        CollectionAssert.IsEmpty(visitor.GetSqlParameters());
    }

    [Test]
    public void Reads_Captured_Template_Argument_Name()
    {
        var argumentName = "value";

        var (sql, parameters) = Visit<NodeDto>(x => x.Path == SqlTemplate.Arg<string>(argumentName));

        Assert.AreEqual("([umbracoNode].[path] = @0)", sql);
        Assert.AreEqual(1, parameters.Length);
        Assert.IsInstanceOf<SqlTemplate.TemplateArg>(parameters[0]);
        Assert.AreEqual("value", ((SqlTemplate.TemplateArg)parameters[0]).Name);
    }

    [Test]
    public void Applies_Sql_Text_With_One_Field()
    {
        Expression<Func<NodeDto, object>> predicate = node =>
            SqlExtensionsStatics.SqlText<bool>(node.Path, n1 => $"({n1} LIKE 'x')");
        var visitor = new PocoToSqlExpressionVisitor<NodeDto>(SqlContext, null);

        Assert.AreEqual("([umbracoNode].[path] LIKE 'x')", visitor.Visit(predicate));
    }

    [Test]
    public void Two_Dto_Visitor_Reads_Captured_Values()
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
    public void Two_Dto_Visitor_Applies_Sql_Text_With_Two_Fields()
    {
        Expression<Func<UserDto, UserGroupDto, object>> predicate = (user, group) =>
            SqlExtensionsStatics.SqlText<bool>(user.Login, group.Name, (n1, n2) => $"({n1} LIKE concat({n2}, ',%'))");
        var visitor = new PocoToSqlExpressionVisitor<UserDto, UserGroupDto>(SqlContext, null, null);

        Assert.AreEqual(
            "([umbracoUser].[userLogin] LIKE concat([umbracoUserGroup].[userGroupName], ',%'))",
            visitor.Visit(predicate));
    }

    [Test]
    public void Three_Dto_Visitor_Reads_Captured_Values()
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
    public void Three_Dto_Visitor_Applies_Sql_Text_With_Three_Fields()
    {
        Expression<Func<NodeDto, UserGroupDto, UserDto, object>> predicate = (node, group, user) =>
            SqlExtensionsStatics.SqlText<bool>(node.Path, group.Name, user.Login, (n1, n2, n3) => $"({n1} = {n2} OR {n1} = {n3})");
        var visitor = new PocoToSqlExpressionVisitor<NodeDto, UserGroupDto, UserDto>(SqlContext, null, null, null);

        Assert.AreEqual(
            "([umbracoNode].[path] = [umbracoUserGroup].[userGroupName] OR [umbracoNode].[path] = [umbracoUser].[userLogin])",
            visitor.Visit(predicate));
    }

    [Test]
    public void Does_Not_Compile_Code_Per_Captured_Value()
        => JitCompilationAssert.DoesNotCompilePerRun(i => Visit<NodeDto>(x => x.NodeId == i));

    private (string Sql, object[] Parameters) Visit<TDto>(Expression<Func<TDto, bool>> predicate)
    {
        var visitor = new PocoToSqlExpressionVisitor<TDto>(SqlContext, null);
        var sql = visitor.Visit(predicate);
        return (sql, visitor.GetSqlParameters());
    }

    private class Filter
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
