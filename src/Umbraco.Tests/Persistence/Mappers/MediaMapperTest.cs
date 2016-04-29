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
            // Act
            string column = new MediaMapper().Map(new SqlCeSyntaxProvider(), "Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Trashed_Property()
        {
            // Act
            string column = new MediaMapper().Map(new SqlCeSyntaxProvider(), "Trashed");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[trashed]"));
        }

        [Test]
        public void Can_Map_UpdateDate_Property()
        {
            // Act
            string column = new MediaMapper().Map(new SqlCeSyntaxProvider(), "UpdateDate");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentVersion].[VersionDate]"));
        }

        [Test]
        public void Can_Map_Version_Property()
        {
            
            // Act
            string column = new MediaMapper().Map(new SqlCeSyntaxProvider(), "Version");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentVersion].[VersionId]"));
        }
    }
}