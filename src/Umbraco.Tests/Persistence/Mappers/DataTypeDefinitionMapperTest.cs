using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class DataTypeDefinitionMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new DataTypeDefinitionMapper().Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[id]"));
        }

        [Test]
        public void Can_Map_Key_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new DataTypeDefinitionMapper().Map("Key");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoNode].[uniqueID]"));
        }

        [Test]
        public void Can_Map_DatabaseType_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new DataTypeDefinitionMapper().Map("DatabaseType");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[dbType]"));
        }

        [Test]
        public void Can_Map_PropertyEditorAlias_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new DataTypeDefinitionMapper().Map("PropertyEditorAlias");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDataType].[propertyEditorAlias]"));
        }
    }
}