using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class RelationTypeMapperTest : MapperTestBase
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Act
            string column = new RelationTypeMapper(MockSqlContext(), CreateMaps()).Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[id]"));
        }

        [Test]
        public void Can_Map_Alias_Property()
        {
            // Act
            string column = new RelationTypeMapper(MockSqlContext(), CreateMaps()).Map("Alias");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[alias]"));
        }

        [Test]
        public void Can_Map_ChildObjectType_Property()
        {

            // Act
            string column = new RelationTypeMapper(MockSqlContext(), CreateMaps()).Map("ChildObjectType");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[childObjectType]"));
        }

        [Test]
        public void Can_Map_IsBidirectional_Property()
        {

            // Act
            string column = new RelationTypeMapper(MockSqlContext(), CreateMaps()).Map("IsBidirectional");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[dual]"));
        }
    }
}
