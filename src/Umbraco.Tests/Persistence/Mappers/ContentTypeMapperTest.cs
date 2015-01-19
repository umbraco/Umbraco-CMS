using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class ContentTypeMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new ContentTypeMapper().Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Name_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new ContentTypeMapper().Map("Name");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[text]"));
        }

        [Test]
        public void Can_Map_Thumbnail_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new ContentTypeMapper().Map("Thumbnail");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentType].[thumbnail]"));
        }

        [Test]
        public void Can_Map_Description_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new ContentTypeMapper().Map("Description");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentType].[description]"));
        }
    }
}