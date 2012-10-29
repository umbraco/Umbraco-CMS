using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class QueryBuilderTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Build_StartsWith_Query_For_IContent()
        {
            // Arrange
            var sql = new Sql();
            sql.Select("*");
            sql.From("umbracoNode");

            var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith("-1"));

            // Act
            var translator = new SqlTranslator<IContent>(sql, query);
            var result = translator.Translate();
            var strResult = result.SQL;

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE (upper([umbracoNode].[path]) like '-1%')";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));
            Console.WriteLine(strResult);
        }

        [Test]
        public void Can_Build_ParentId_Query_For_IContent()
        {
            // Arrange
            var sql = new Sql();
            sql.Select("*");
            sql.From("umbracoNode");

            var query = Query<IContent>.Builder.Where(x => x.ParentId == -1);

            // Act
            var translator = new SqlTranslator<IContent>(sql, query);
            var result = translator.Translate();
            var strResult = result.SQL;

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE (([umbracoNode].[parentID]=-1))";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));
            Console.WriteLine(strResult);
        }

        [Test]
        public void Can_Build_ContentTypeAlias_Query_For_IContentType()
        {
            // Arrange
            var sql = new Sql();
            sql.Select("*");
            sql.From("umbracoNode");

            var query = Query<IContentType>.Builder.Where(x => x.Alias == "umbTextpage");

            // Act
            var translator = new SqlTranslator<IContentType>(sql, query);
            var result = translator.Translate();
            var strResult = result.SQL;

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE (([cmsContentType].[alias]='umbTextpage'))";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));
            Console.WriteLine(strResult);
        }
    }
}