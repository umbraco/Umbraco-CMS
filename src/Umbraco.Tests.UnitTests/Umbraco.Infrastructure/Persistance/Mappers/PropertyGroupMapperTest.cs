using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class PropertyGroupMapperTest : MapperTestBase
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Act
            string column = new PropertyGroupMapper(MockSqlContext(), CreateMaps()).Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[id]"));
        }

        [Test]
        public void Can_Map_SortOrder_Property()
        {
            // Act
            string column = new PropertyGroupMapper(MockSqlContext(), CreateMaps()).Map("SortOrder");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[sortorder]"));
        }

        [Test]
        public void Can_Map_Name_Property()
        {
            // Act
            string column = new PropertyGroupMapper(MockSqlContext(), CreateMaps()).Map("Name");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyTypeGroup].[text]"));
        }
    }
}
