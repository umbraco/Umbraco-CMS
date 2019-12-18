using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class ContentTypeMapperTest : MapperTestBase
    {
        [Test]
        public void Can_Map_Id_Property()
        {

            // Act
            string column = new ContentTypeMapper(MockSqlContext(), CreateMaps()).Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Name_Property()
        {

            // Act
            string column = new ContentTypeMapper(MockSqlContext(), CreateMaps()).Map("Name");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[text]"));
        }

        [Test]
        public void Can_Map_Thumbnail_Property()
        {

            // Act
            string column = new ContentTypeMapper(MockSqlContext(), CreateMaps()).Map("Thumbnail");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentType].[thumbnail]"));
        }

        [Test]
        public void Can_Map_Description_Property()
        {

            // Act
            string column = new ContentTypeMapper(MockSqlContext(), CreateMaps()).Map("Description");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsContentType].[description]"));
        }
    }
}
