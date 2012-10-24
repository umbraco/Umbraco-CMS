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
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = PropertyTypeMapper.Instance.Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[id]"));
        }

        [Test]
        public void Can_Map_Alias_Property()
        {
            // Arrange
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = PropertyTypeMapper.Instance.Map("Alias");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[Alias]"));
        }

        [Test]
        public void Can_Map_DataTypeId_Property()
        {
            // Arrange
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = PropertyTypeMapper.Instance.Map("DataTypeId");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[dataTypeId]"));
        }

        [Test]
        public void Can_Map_SortOrder_Property()
        {
            // Arrange
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = PropertyTypeMapper.Instance.Map("SortOrder");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsPropertyType].[sortOrder]"));
        }

        [Test]
        public void Can_Map_DataTypeControlId_Property()
        {
            // Arrange
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = PropertyTypeMapper.Instance.Map("DataTypeControlId");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[controlId]"));
        }

        [Test]
        public void Can_Map_DataTypeDatabaseType_Property()
        {
            // Arrange
            SyntaxConfig.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = PropertyTypeMapper.Instance.Map("DataTypeDatabaseType");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[dbType]"));
        }
    }
}