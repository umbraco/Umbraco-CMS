using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class PropertyTypeMapperTest : MapperTestBase
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Act
            string column = new PropertyTypeMapper(MockSqlContext(), CreateMaps()).Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[id]"));
        }

        [Test]
        public void Can_Map_Alias_Property()
        {
            // Act
            string column = new PropertyTypeMapper(MockSqlContext(), CreateMaps()).Map("Alias");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[Alias]"));
        }

        [Test]
        public void Can_Map_DataTypeDefinitionId_Property()
        {
            // Act
            string column = new PropertyTypeMapper(MockSqlContext(), CreateMaps()).Map("DataTypeId");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[dataTypeId]"));
        }

        [Test]
        public void Can_Map_SortOrder_Property()
        {
            // Act
            string column = new PropertyTypeMapper(MockSqlContext(), CreateMaps()).Map("SortOrder");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[sortOrder]"));
        }

        [Test]
        public void Can_Map_PropertyEditorAlias_Property()
        {
            // Act
            string column = new PropertyTypeMapper(MockSqlContext(), CreateMaps()).Map("PropertyEditorAlias");

            // Assert
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[propertyEditorAlias]"));
        }

        [Test]
        public void Can_Map_DataTypeDatabaseType_Property()
        {
            // Act
            string column = new PropertyTypeMapper(MockSqlContext(), CreateMaps()).Map("ValueStorageType");

            // Assert
            Assert.That(column, Is.EqualTo($"[{Constants.DatabaseSchema.Tables.DataType}].[dbType]"));
        }
    }
}
