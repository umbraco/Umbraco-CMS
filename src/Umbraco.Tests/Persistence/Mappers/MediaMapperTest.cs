using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class MediaMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new MediaMapper().Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Trashed_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new MediaMapper().Map("Trashed");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[trashed]"));
        }

        [Test]
        public void Can_Map_UpdateDate_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new MediaMapper().Map("UpdateDate");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentVersion].[VersionDate]"));
        }

        [Test]
        public void Can_Map_Version_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new MediaMapper().Map("Version");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentVersion].[VersionId]"));
        }
    }
}