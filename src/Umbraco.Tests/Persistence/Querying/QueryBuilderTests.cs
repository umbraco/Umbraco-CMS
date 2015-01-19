using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
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

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE (upper([umbracoNode].[path]) LIKE upper(@0))";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));

            Assert.AreEqual(1, result.Arguments.Length);
            Assert.AreEqual("-1%", sql.Arguments[0]);

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

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE ([umbracoNode].[parentID] = @0)";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));

            Assert.AreEqual(1, result.Arguments.Length);
            Assert.AreEqual(-1, sql.Arguments[0]);

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

            string expectedResult = "SELECT *\nFROM umbracoNode\nWHERE ([cmsContentType].[alias] = @0)";

            // Assert
            Assert.That(strResult, Is.Not.Empty);
            Assert.That(strResult, Is.EqualTo(expectedResult));
            Assert.AreEqual(1, result.Arguments.Length);
            Assert.AreEqual("umbTextpage", sql.Arguments[0]);

            Console.WriteLine(strResult);
        }

        [Test]
        public void Can_Build_PublishedDescendants_Query_For_IContent()
        {
            // Arrange
            var path = "-1,1046,1076,1089";
            var id = 1046;
            var nodeObjectTypeId = new Guid(Constants.ObjectTypes.Document);

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == nodeObjectTypeId);

            var query = Query<IContent>.Builder.Where(x => x.Path.StartsWith(path) && x.Id != id && x.Published == true && x.Trashed == false);

            // Act
            var translator = new SqlTranslator<IContent>(sql, query);
            var result = translator.Translate();
            var strResult = result.SQL;

            // Assert
            Console.WriteLine(strResult);
        }
    }
}