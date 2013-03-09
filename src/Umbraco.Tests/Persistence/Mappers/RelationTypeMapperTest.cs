using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class RelationTypeMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationTypeMapper.Instance.Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[id]"));
        }

        [Test]
        public void Can_Map_Alias_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationTypeMapper.Instance.Map("Alias");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[alias]"));
        }

        [Test]
        public void Can_Map_ChildObjectType_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationTypeMapper.Instance.Map("ChildObjectType");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[childObjectType]"));
        }

        [Test]
        public void Can_Map_IsBidirectional_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationTypeMapper.Instance.Map("IsBidirectional");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelationType].[dual]"));
        }
    }
}