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
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new RelationMapper().Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[id]"));
        }

        [Test]
        public void Can_Map_ChildId_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new RelationMapper().Map("ChildId");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[childId]"));
        }

        [Test]
        public void Can_Map_Datetime_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new RelationMapper().Map("CreateDate");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[datetime]"));
        }

        [Test]
        public void Can_Map_Comment_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new RelationMapper().Map("Comment");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[comment]"));
        }

        [Test]
        public void Can_Map_RelationType_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new RelationMapper().Map("RelationTypeId");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[relType]"));
        }
    }
}