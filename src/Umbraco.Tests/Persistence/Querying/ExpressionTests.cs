using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class ExpressionTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Path_StartsWith_Predicate_In_Same_Result()
        {
            //Arrange
            Expression<Func<IContent, bool>> predicate = content => content.Path.StartsWith("-1");
            var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("upper([umbracoNode].[path]) like '-1%'", result);
        }

        [Test]
        public void Can_Verify_ParentId_StartsWith_Predicate_In_Same_Result()
        {
            //Arrange
            Expression<Func<IContent, bool>> predicate = content => content.ParentId == -1;
            var modelToSqlExpressionHelper = new ModelToSqlExpressionHelper<IContent>();
            var result = modelToSqlExpressionHelper.Visit(predicate);

            Console.WriteLine("Model to Sql ExpressionHelper: \n" + result);

            Assert.AreEqual("[umbracoNode].[parentID] = -1", result);
        }
    }
}