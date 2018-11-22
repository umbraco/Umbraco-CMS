using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class ExpressionTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Equals_Claus_With_Two_Entity_Values()
        {
            var dataType = new DataTypeDefinition(-1, "Test")
            {
                Id = 12345
            };
            Expression<Func<PropertyType, bool>> predicate = p => p.DataTypeDefinitionId == dataType.Id;
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<PropertyType>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("([cmsPropertyType].[dataTypeId] = @0)", result);
            Assert.AreEqual(12345, modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Can_Query_With_Content_Type_Alias()
        {
            //Arrange
            Expression<Func<IMedia, bool>> predicate = content => content.ContentType.Alias == "Test";
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("([cmsContentType].[alias] = @0)", result);
            Assert.AreEqual("Test", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Can_Query_With_Content_Type_Aliases()
        {
            //Arrange
            var aliases = new[] {"Test1", "Test2"};
            Expression<Func<IMedia, bool>> predicate = content => aliases.Contains(content.ContentType.Alias);
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("[cmsContentType].[alias] IN (@1,@2)", result);
            Assert.AreEqual("Test1", modelToSqlExpressionHelper.GetSqlParameters()[1]);
            Assert.AreEqual("Test2", modelToSqlExpressionHelper.GetSqlParameters()[2]);
        }

        [Test]
        public void CachedExpression_Can_Verify_Path_StartsWith_Predicate_In_Same_Result()
        {
            //Arrange

            //use a single cached expression for multiple expressions and ensure the correct output
            // is done for both of them.
            var cachedExpression = new CachedExpression();


            Expression<Func<IContent, bool>> predicate1 = content => content.Path.StartsWith("-1");
            cachedExpression.Wrap(predicate1);
            var modelToSqlExpressionHelper1 = new ModelToSqlExpressionVisitor<IContent>();
            var result1 = modelToSqlExpressionHelper1.Visit(cachedExpression);
            Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", result1);
            Assert.AreEqual("-1%", modelToSqlExpressionHelper1.GetSqlParameters()[0]);

            Expression<Func<IContent, bool>> predicate2 = content => content.Path.StartsWith("-1,123,97");
            cachedExpression.Wrap(predicate2);
            var modelToSqlExpressionHelper2 = new ModelToSqlExpressionVisitor<IContent>();
            var result2 = modelToSqlExpressionHelper2.Visit(cachedExpression);
            Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", result2);
            Assert.AreEqual("-1,123,97%", modelToSqlExpressionHelper2.GetSqlParameters()[0]);

        }

        [Test]
        public void Can_Verify_Path_StartsWith_Predicate_In_Same_Result()
        {
            //Arrange
            Expression<Func<IContent, bool>> predicate = content => content.Path.StartsWith("-1");
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);
            
            Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", result);
            Assert.AreEqual("-1%", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Can_Verify_ParentId_StartsWith_Predicate_In_Same_Result()
        {
            //Arrange
            Expression<Func<IContent, bool>> predicate = content => content.ParentId == -1;
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("([umbracoNode].[parentID] = @0)", result);
            Assert.AreEqual(-1, modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Equals_Operator_For_Value_Gets_Escaped()
        {
            Expression<Func<IUser, bool>> predicate = user => user.Username == "hello@world.com";
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("([umbracoUser].[userLogin] = @0)", result);
            Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Equals_Method_For_Value_Gets_Escaped()
        {
            Expression<Func<IUser, bool>> predicate = user => user.Username.Equals("hello@world.com");
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper([umbracoUser].[userLogin]) = upper(@0)", result);
            Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Model_Expression_Value_Does_Not_Get_Double_Escaped()
        {
            //mysql escapes backslashes, so we'll test with that
            SqlSyntaxContext.SqlSyntaxProvider = new MySqlSyntaxProvider(Mock.Of<ILogger>());

            Expression<Func<IUser, bool>> predicate = user => user.Username.Equals("mydomain\\myuser");
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper(`umbracoUser`.`userLogin`) = upper(@0)", result);
            Assert.AreEqual("mydomain\\myuser", modelToSqlExpressionHelper.GetSqlParameters()[0]);
            
        }

        [Test]
        public void Poco_Expression_Value_Does_Not_Get_Double_Escaped()
        {
            //mysql escapes backslashes, so we'll test with that
            SqlSyntaxContext.SqlSyntaxProvider = new MySqlSyntaxProvider(Mock.Of<ILogger>());

            Expression<Func<UserDto, bool>> predicate = user => user.Login.StartsWith("mydomain\\myuser");
            var modelToSqlExpressionHelper = new PocoToSqlExpressionVisitor<UserDto>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Poco to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper(`umbracoUser`.`userLogin`) LIKE upper(@0)", result);
            Assert.AreEqual("mydomain\\myuser%", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Sql_Replace_Mapped()
        {
            Expression<Func<IUser, bool>> predicate = user => user.Username.Replace("@world", "@test") == "hello@test.com";
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>();
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
            var userNames = new[] {"hello@world.com", "blah@blah.com"};

            Expression<Func<IUser, bool>> predicate = user => userNames.Contains(user.Username);
            var modelToSqlExpressionHelper = new ModelToSqlExpressionVisitor<IUser>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Debug.Print("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("[umbracoUser].[userLogin] IN (@1,@2)", result);
            Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[1]);
            Assert.AreEqual("blah@blah.com", modelToSqlExpressionHelper.GetSqlParameters()[2]);
        }

    }
}
