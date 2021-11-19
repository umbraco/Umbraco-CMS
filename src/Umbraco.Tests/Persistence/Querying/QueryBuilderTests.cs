using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
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
            var sql = Sql();
            sql.SelectAll();
            sql.From("umbracoNode");

            var query = new Query<IContent>(SqlContext).Where(x => x.Path.StartsWith("-1"));

            // Act
            var translator = new SqlTranslator<IContent>(sql, query);
            var result = translator.Translate();
            var strResult = result.SQL;

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE (upper([umbracoNode].[path]) LIKE upper(@0))";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));

            Assert.AreEqual(1, result.Arguments.Length);
            Assert.AreEqual("-1%", sql.Arguments[0]);

            Debug.Print(strResult);
        }

        [Test]
        public void Can_Build_ParentId_Query_For_IContent()
        {
            // Arrange
            var sql = Sql();
            sql.SelectAll();
            sql.From("umbracoNode");

            var query = new Query<IContent>(SqlContext).Where(x => x.ParentId == -1);

            // Act
            var translator = new SqlTranslator<IContent>(sql, query);
            var result = translator.Translate();
            var strResult = result.SQL;

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE (([umbracoNode].[parentId] = @0))";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));

            Assert.AreEqual(1, result.Arguments.Length);
            Assert.AreEqual(-1, sql.Arguments[0]);

            Debug.Print(strResult);
        }

        [Test]
        public void Can_Build_ContentTypeAlias_Query_For_IContentType()
        {
            // Arrange
            var sql = Sql();
            sql.SelectAll();
            sql.From("umbracoNode");

            var query = new Query<IContentType>(SqlContext).Where(x => x.Alias == "umbTextpage");

            // Act
            var translator = new SqlTranslator<IContentType>(sql, query);
            var result = translator.Translate();
            var strResult = result.SQL;

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE (([cmsContentType].[alias] = @0))";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));
            Assert.AreEqual(1, result.Arguments.Length);
            Assert.AreEqual("umbTextpage", sql.Arguments[0]);

            Debug.Print(strResult);
        }

        [Test]
        public void Can_Build_PublishedDescendants_Query_For_IContent()
        {
            const string path = "-1,1046,1076,1089";
            const int id = 1046;

            var sql = Sql();
            sql.SelectAll()
                .From<DocumentDto>(); // the actual SELECT really does not matter

            var query = SqlContext.Query<IContent>().Where(x => x.Path.StartsWith(path) && x.Id != id && x.Published && x.Trashed == false);

            var translator = new SqlTranslator<IContent>(sql, query);
            var result = translator.Translate();

            Assert.AreEqual("-1,1046,1076,1089%", result.Arguments[0]);
            Assert.AreEqual(1046, result.Arguments[1]);
            Assert.AreEqual(true, result.Arguments[2]);
            Assert.AreEqual(false, result.Arguments[3]);
        }
    }
}
