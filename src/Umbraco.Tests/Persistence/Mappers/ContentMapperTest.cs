using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class ContentMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {

            // Act
            string column = new ContentMapper().Map(new SqlCeSyntaxProvider(), "Id");


            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Trashed_Property()
        {

            // Act
            string column = new ContentMapper().Map(new SqlCeSyntaxProvider(), "Trashed");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[trashed]"));
        }

        [Test]
        public void Can_Map_Published_Property()
        {

            // Act
            string column = new ContentMapper().Map(new SqlCeSyntaxProvider(), "Published");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDocument].[published]"));
        }

        [Test]
        public void Can_Map_Version_Property()
        {

            // Act
            string column = new ContentMapper().Map(new SqlCeSyntaxProvider(), "Version");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentVersion].[VersionId]"));
        }
    }
}