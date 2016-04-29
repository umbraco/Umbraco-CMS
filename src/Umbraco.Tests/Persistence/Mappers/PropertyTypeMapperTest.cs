using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class PropertyTypeMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Act
            string column = new PropertyTypeMapper().Map(new SqlCeSyntaxProvider(), "Id");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[id]"));
        }

        [Test]
        public void Can_Map_Alias_Property()
        {
            // Act
            string column = new PropertyTypeMapper().Map(new SqlCeSyntaxProvider(), "Alias");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[Alias]"));
        }

        [Test]
        public void Can_Map_DataTypeDefinitionId_Property()
        {
            // Act
            string column = new PropertyTypeMapper().Map(new SqlCeSyntaxProvider(), "DataTypeDefinitionId");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[dataTypeId]"));
        }

        [Test]
        public void Can_Map_SortOrder_Property()
        {
            // Act
            string column = new PropertyTypeMapper().Map(new SqlCeSyntaxProvider(), "SortOrder");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[sortOrder]"));
        }

        [Test]
        public void Can_Map_PropertyEditorAlias_Property()
        {
            // Act
            string column = new PropertyTypeMapper().Map(new SqlCeSyntaxProvider(), "PropertyEditorAlias");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[propertyEditorAlias]"));
        }

        [Test]
        public void Can_Map_DataTypeDatabaseType_Property()
        {
            // Act
            string column = new PropertyTypeMapper().Map(new SqlCeSyntaxProvider(), "DataTypeDatabaseType");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[dbType]"));
        }
    }
}