// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

/// <summary>
///     Tests for <see cref="ModelToSqlExpressionVisitor{T}" />, including the string comparison, <c>Replace</c> and
///     <c>IN</c> handling it inherits from <see cref="ExpressionVisitorBase" />.
/// </summary>
[TestFixture]
public class ModelToSqlExpressionVisitorTests : BaseUsingSqlSyntax
{
    private static int _staticParentId = -20;

    [Test]
    public void Reads_Captured_Local()
    {
        var parentId = 1234;

        var (sql, parameters) = Visit<IContent>(x => x.ParentId == parentId);

        Assert.AreEqual("([umbracoNode].[parentId] = @0)", sql);
        CollectionAssert.AreEqual(new object[] { 1234 }, parameters);
    }

    [Test]
    public void Reads_Member_Of_Captured_Object()
    {
        var filter = new Filter { Id = 99, Inner = new Filter { Name = "inner" } };

        var (sql, parameters) = Visit<IContent>(x => x.ParentId == filter.Id && x.Name == filter.Inner.Name);

        Assert.AreEqual("(([umbracoNode].[parentId] = @0) AND ([umbracoContentVersion].[text] = @1))", sql);
        CollectionAssert.AreEqual(new object[] { 99, "inner" }, parameters);
    }

    [Test]
    public void Reads_Static_Member()
    {
        var (sql, parameters) = Visit<IContent>(x => x.ParentId == _staticParentId);

        Assert.AreEqual("([umbracoNode].[parentId] = @0)", sql);
        CollectionAssert.AreEqual(new object[] { -20 }, parameters);
    }

    [Test]
    public void Reads_Captured_Nullable()
    {
        int? creatorId = 5;

        var (sql, parameters) = Visit<IContent>(x => x.CreatorId == creatorId);

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
    public void Reads_Captured_String_Comparison_Argument(string method, string expectedSql, string expectedParameter)
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

        var (sql, parameters) = Visit(predicate);

        Assert.AreEqual(expectedSql, sql);
        CollectionAssert.AreEqual(new object[] { expectedParameter }, parameters);
    }

    [Test]
    public void Reads_Computed_String_Comparison_Argument()
    {
        var parent = new Filter { Name = "-1,123" };

        var (sql, parameters) = Visit<IContent>(x => x.Path.StartsWith(parent.Name + ","));

        Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", sql);
        CollectionAssert.AreEqual(new object[] { "-1,123,%" }, parameters);
    }

    [Test]
    public void Reads_Captured_Replace_Arguments()
    {
        var search = "@world";
        var replacement = new Filter { Name = "@test" };

        var (sql, parameters) = Visit<IUser>(x => x.Username.Replace(search, replacement.Name) == "hello@test.com");

        Assert.AreEqual("(replace([umbracoUser].[userLogin], @1, @2) = @0)", sql);
        CollectionAssert.AreEqual(new object[] { "hello@test.com", "@world", "@test" }, parameters);
    }

    [Test]
    public void Reads_Captured_Collection_For_In_Clause()
    {
        var filter = new Filter { Names = ["a@b.com", "c@d.com"] };

        var (sql, parameters) = Visit<IUser>(x => filter.Names.Contains(x.Username));

        Assert.AreEqual("[umbracoUser].[userLogin] IN (@1,@2)", sql);
        Assert.AreEqual("a@b.com", parameters[1]);
        Assert.AreEqual("c@d.com", parameters[2]);
    }

    [Test]
    public void Writes_Null_For_Empty_Captured_Collection_In_Clause()
    {
        var names = new List<string>();

        var (sql, _) = Visit<IUser>(x => names.Contains(x.Username));

        Assert.AreEqual("[umbracoUser].[userLogin] IN (NULL)", sql);
    }

    [Test]
    public void Does_Not_Compile_Code_Per_Captured_Value()
        => JitCompilationAssert.DoesNotCompilePerRun(i =>
        {
            var parent = new Filter { Name = "-1," + i };
            Visit<IContent>(x => x.ParentId == i && x.Path.StartsWith(parent.Name!));
        });

    private (string Sql, object[] Parameters) Visit<T>(Expression<Func<T, bool>> predicate)
    {
        var visitor = new ModelToSqlExpressionVisitor<T>(SqlContext.SqlSyntax, Mappers);
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
