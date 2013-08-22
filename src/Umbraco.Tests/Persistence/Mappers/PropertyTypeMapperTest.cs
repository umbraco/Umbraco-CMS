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
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = new PropertyTypeMapper().Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[id]"));
        }

        [Test]
        public void Can_Map_Alias_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = new PropertyTypeMapper().Map("Alias");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[Alias]"));
        }

        [Test]
        public void Can_Map_DataTypeDefinitionId_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = new PropertyTypeMapper().Map("DataTypeDefinitionId");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[dataTypeId]"));
        }

        [Test]
        public void Can_Map_SortOrder_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = new PropertyTypeMapper().Map("SortOrder");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[sortOrder]"));
        }

        [Test]
        public void Can_Map_DataTypeControlId_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = new PropertyTypeMapper().Map("DataTypeId");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[controlId]"));
        }

        [Test]
        public void Can_Map_DataTypeDatabaseType_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = new PropertyTypeMapper().Map("DataTypeDatabaseType");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[dbType]"));
        }
    }
}