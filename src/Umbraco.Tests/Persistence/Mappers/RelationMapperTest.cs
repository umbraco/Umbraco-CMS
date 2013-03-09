using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class RelationMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationMapper.Instance.Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[id]"));
        }

        [Test]
        public void Can_Map_ChildId_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationMapper.Instance.Map("ChildId");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[childId]"));
        }

        [Test]
        public void Can_Map_Datetime_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationMapper.Instance.Map("CreateDate");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[datetime]"));
        }

        [Test]
        public void Can_Map_Comment_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationMapper.Instance.Map("Comment");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[comment]"));
        }

        [Test]
        public void Can_Map_RelationType_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = RelationMapper.Instance.Map("RelationTypeId");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[relType]"));
        }
    }
}