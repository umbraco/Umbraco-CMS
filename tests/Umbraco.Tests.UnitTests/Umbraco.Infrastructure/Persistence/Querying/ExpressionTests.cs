// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

[TestFixture]
public class ExpressionTests : BaseUsingSqlSyntax
{
    [Test]
    public void Equals_Claus_With_Two_Entity_Values()
    {
        var dataType = new DataType(
                new VoidEditor(Mock.Of<IDataValueEditorFactory>()),
                new ConfigurationEditorJsonSerializer())
        { Id = 12345 };
        Expression<Func<PropertyType, bool>> predicate = p => p.DataTypeId == dataType.Id;
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<PropertyType>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("([cmsPropertyType].[dataTypeId] = @0)", result);
        Assert.AreEqual(12345, modelToSqlExpressionHelper.GetSqlParameters()[0]);
    }

    [Test]
    public void Can_Query_With_Content_Type_Alias()
    {
        // Arrange
        Expression<Func<IMedia, bool>> predicate = content => content.ContentType.Alias == "Test";
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("([cmsContentType].[alias] = @0)", result);
        Assert.AreEqual("Test", modelToSqlExpressionHelper.GetSqlParameters()[0]);
    }

    [Test]
    public void Can_Query_With_Content_Type_Aliases_IEnumerable()
    {
        // Arrange - Contains is IEnumerable.Contains extension method
        var aliases = new[] { "Test1", "Test2" };
        Expression<Func<IMedia, bool>> predicate = content => aliases.Contains(content.ContentType.Alias);
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("[cmsContentType].[alias] IN (@1,@2)", result);
        Assert.AreEqual("Test1", modelToSqlExpressionHelper.GetSqlParameters()[1]);
        Assert.AreEqual("Test2", modelToSqlExpressionHelper.GetSqlParameters()[2]);
    }

    [Test]
    public void Can_Query_With_Content_Type_Aliases_List()
    {
        // Arrange - Contains is List.Contains instance method
        var aliases = new List<string> { "Test1", "Test2" };
        Expression<Func<IMedia, bool>> predicate = content => aliases.Contains(content.ContentType.Alias);
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("[cmsContentType].[alias] IN (@1,@2)", result);
        Assert.AreEqual("Test1", modelToSqlExpressionHelper.GetSqlParameters()[1]);
        Assert.AreEqual("Test2", modelToSqlExpressionHelper.GetSqlParameters()[2]);
    }

    [Test]
    public void CachedExpression_Can_Verify_Path_StartsWith_Predicate_In_Same_Result()
    {
        // Arrange

        // use a single cached expression for multiple expressions and ensure the correct output
        // is done for both of them.
        var cachedExpression = new CachedExpression();

        Expression<Func<IContent, bool>> predicate1 = content => content.Path.StartsWith("-1");
        cachedExpression.Wrap(predicate1);
        var modelToSqlExpressionHelper1 = new ModelToSqlExpressionVisitor<IContent>(SqlContext.SqlSyntax, Mappers);
        var result1 = modelToSqlExpressionHelper1.Visit(cachedExpression);
        Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", result1);
        Assert.AreEqual("-1%", modelToSqlExpressionHelper1.GetSqlParameters()[0]);

        Expression<Func<IContent, bool>> predicate2 = content => content.Path.StartsWith("-1,123,97");
        cachedExpression.Wrap(predicate2);
        var modelToSqlExpressionHelper2 = new ModelToSqlExpressionVisitor<IContent>(SqlContext.SqlSyntax, Mappers);
        var result2 = modelToSqlExpressionHelper2.Visit(cachedExpression);
        Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", result2);
        Assert.AreEqual("-1,123,97%", modelToSqlExpressionHelper2.GetSqlParameters()[0]);
    }

    [Test]
    public void Can_Verify_Path_StartsWith_Predicate_In_Same_Result()
    {
        // Arrange
        Expression<Func<IContent, bool>> predicate = content => content.Path.StartsWith("-1");
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", result);
        Assert.AreEqual("-1%", modelToSqlExpressionHelper.GetSqlParameters()[0]);
    }

    [Test]
    public void Can_Verify_ParentId_StartsWith_Predicate_In_Same_Result()
    {
        // Arrange
        Expression<Func<IContent, bool>> predicate = content => content.ParentId == -1;
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("([umbracoNode].[parentId] = @0)", result);
        Assert.AreEqual(-1, modelToSqlExpressionHelper.GetSqlParameters()[0]);
    }

    [Test]
    public void Equals_Operator_For_Value_Gets_Escaped()
    {
        Expression<Func<IUser, bool>> predicate = user => user.Username == "hello@world.com";
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("([umbracoUser].[userLogin] = @0)", result);
        Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[0]);
    }

    [Test]
    public void Equals_Method_For_Value_Gets_Escaped()
    {
        Expression<Func<IUser, bool>> predicate = user => user.Username.Equals("hello@world.com");
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("upper([umbracoUser].[userLogin]) = upper(@0)", result);
        Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[0]);
    }

    [Test]
    public void Sql_Replace_Mapped()
    {
        Expression<Func<IUser, bool>> predicate = user => user.Username.Replace("@world", "@test") == "hello@test.com";
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("(replace([umbracoUser].[userLogin], @1, @2) = @0)", result);
        Assert.AreEqual("hello@test.com", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        Assert.AreEqual("@world", modelToSqlExpressionHelper.GetSqlParameters()[1]);
        Assert.AreEqual("@test", modelToSqlExpressionHelper.GetSqlParameters()[2]);
    }

    [Test]
    public void Sql_In()
    {
        var userNames = new[] { "hello@world.com", "blah@blah.com" };

        Expression<Func<IUser, bool>> predicate = user => userNames.Contains(user.Username);
        var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>(SqlContext.SqlSyntax, Mappers);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Debug.Print("Model to Sql ExpressionHelper: \n" + result);

        Assert.AreEqual("[umbracoUser].[userLogin] IN (@1,@2)", result);
        Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[1]);
        Assert.AreEqual("blah@blah.com", modelToSqlExpressionHelper.GetSqlParameters()[2]);
    }

    private string GetSomeValue(string s) => "xx" + s + "xx";

    private class Foo
    {
        public string Value { get; set; }
    }

    [Test]
    public void StartsWith()
    {
        Expression<Func<UserDto, object>> predicate = user => user.Login.StartsWith("aaaaa");
        var modelToSqlExpressionHelper = new PocoToSqlExpressionVisitor<UserDto>(SqlContext, null);
        var result = modelToSqlExpressionHelper.Visit(predicate);

        Console.WriteLine(result);
        Assert.AreEqual("upper([umbracoUser].[userLogin]) LIKE upper(@0)", result);

        predicate = user => user.Login.StartsWith(GetSomeValue("aaaaa"));
        modelToSqlExpressionHelper = new PocoToSqlExpressionVisitor<UserDto>(SqlContext, null);
        result = modelToSqlExpressionHelper.Visit(predicate);

        Console.WriteLine(result);
        Assert.AreEqual("upper([umbracoUser].[userLogin]) LIKE upper(@0)", result);

        predicate = user => user.Login.StartsWith(GetSomeValue("aaaaa"));
        modelToSqlExpressionHelper = new PocoToSqlExpressionVisitor<UserDto>(SqlContext, null);
        result = modelToSqlExpressionHelper.Visit(predicate);

        Console.WriteLine(result);
        Assert.AreEqual("upper([umbracoUser].[userLogin]) LIKE upper(@0)", result);

        var foo = new Foo { Value = "aaaaa" };
        predicate = user => user.Login.StartsWith(foo.Value);
        modelToSqlExpressionHelper = new PocoToSqlExpressionVisitor<UserDto>(SqlContext, null);
        result = modelToSqlExpressionHelper.Visit(predicate);

        Console.WriteLine(result);
        Assert.AreEqual("upper([umbracoUser].[userLogin]) LIKE upper(@0)", result);

        // below does not work, we want to output
        // LIKE concat([group].[name], ',%')
        // and how would we get the comma there? we'd have to parse .StartsWith(group.Name + ',')
        // which is going to be quite complicated => no

        // how can we do user.Login.StartsWith(other.Value) ?? to use in WHERE or JOIN.ON clauses ??
        //// Expression<Func<UserDto, UserGroupDto, object>> predicate2 = (user, group) => user.Login.StartsWith(group.Name);
        //// var modelToSqlExpressionHelper2 = new PocoToSqlExpressionVisitor<UserDto, UserGroupDto>(SqlContext, null, null);
        //// var result2 = modelToSqlExpressionHelper2.Visit(predicate2); // fails, for now

        //// Console.WriteLine(result2);

        Expression<Func<UserDto, UserGroupDto, object>> predicate3 = (user, group) =>
            SqlExtensionsStatics.SqlText<bool>(user.Login, group.Name, (n1, n2) => $"({n1} LIKE concat({n2}, ',%'))");
        var modelToSqlExpressionHelper3 = new PocoToSqlExpressionVisitor<UserDto, UserGroupDto>(SqlContext, null, null);
        var result3 = modelToSqlExpressionHelper3.Visit(predicate3);

        Console.WriteLine(result3);
    }
}
