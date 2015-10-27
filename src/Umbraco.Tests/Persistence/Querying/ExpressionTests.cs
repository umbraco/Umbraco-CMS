using System;
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
    //    [Test]
    //    public void Can_Query_With_Content_Type_Alias()
    //    {
    //        //Arrange
    //        Expression<Func<IMedia, bool>> predicate = content => content.ContentType.Alias == "Test";
    //        var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IContent>();
    //        var result = modelToSqlExpressionHelper.Visit(predicate);

    //        Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

    //        Assert.AreEqual("[cmsContentType].[alias] = @0", result);
    //        Assert.AreEqual("Test", modelToSqlExpressionHelper.GetSqlParameters()[0]);
    //    }

        [Test]
        public void Can_Verify_Path_StartsWith_Predicate_In_Same_Result()
        {
            //Arrange
            Expression<Func<IContent, bool>> predicate = content => content.Path.StartsWith("-1");
            var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper([umbracoNode].[path]) LIKE upper(@0)", result);
            Assert.AreEqual("-1%", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Can_Verify_ParentId_StartsWith_Predicate_In_Same_Result()
        {
            //Arrange
            Expression<Func<IContent, bool>> predicate = content => content.ParentId == -1;
            var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("[umbracoNode].[parentID] = @0", result);
            Assert.AreEqual(-1, modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Equals_Operator_For_Value_Gets_Escaped()
        {
            Expression<Func<IUser, bool>> predicate = user => user.Username == "hello@world.com";
            var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IUser>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("[umbracoUser].[userLogin] = @0", result);
            Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Equals_Method_For_Value_Gets_Escaped()
        {
            Expression<Func<IUser, bool>> predicate = user => user.Username.Equals("hello@world.com");
            var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IUser>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper([umbracoUser].[userLogin]) = upper(@0)", result);
            Assert.AreEqual("hello@world.com", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

        [Test]
        public void Model_Expression_Value_Does_Not_Get_Double_Escaped()
        {
            //mysql escapes backslashes, so we'll test with that
            SqlSyntaxContext.SqlSyntaxProvider = new MySqlSyntaxProvider(Mock.Of<ILogger>());

            Expression<Func<IUser, bool>> predicate = user => user.Username.Equals("mydomain\\myuser");
            var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IUser>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper(`umbracoUser`.`userLogin`) = upper(@0)", result);
            Assert.AreEqual("mydomain\\myuser", modelToSqlExpressionHelper.GetSqlParameters()[0]);
            
        }

        [Test]
        public void Poco_Expression_Value_Does_Not_Get_Double_Escaped()
        {
            //mysql escapes backslashes, so we'll test with that
            SqlSyntaxContext.SqlSyntaxProvider = new MySqlSyntaxProvider(Mock.Of<ILogger>());

            Expression<Func<UserDto, bool>> predicate = user => user.Login.StartsWith("mydomain\\myuser");
            var modelToSqlExpressionHelper = new PocoToSqlExpressionHelper<UserDto>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Poco to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper(`umbracoUser`.`userLogin`) LIKE upper(@0)", result);
            Assert.AreEqual("mydomain\\myuser%", modelToSqlExpressionHelper.GetSqlParameters()[0]);
        }

    }
}