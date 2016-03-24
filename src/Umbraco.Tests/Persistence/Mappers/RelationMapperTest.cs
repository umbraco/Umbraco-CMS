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
            // Act
            string column = new RelationMapper(new SqlCeSyntaxProvider()).Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[id]"));
        }

        [Test]
        public void Can_Map_ChildId_Property()
        {
            // Act
            string column = new RelationMapper(new SqlCeSyntaxProvider()).Map("ChildId");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[childId]"));
        }

        [Test]
        public void Can_Map_Datetime_Property()
        {
            // Act
            string column = new RelationMapper(new SqlCeSyntaxProvider()).Map("CreateDate");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[datetime]"));
        }

        [Test]
        public void Can_Map_Comment_Property()
        {
            // Act
            string column = new RelationMapper(new SqlCeSyntaxProvider()).Map("Comment");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[comment]"));
        }

        [Test]
        public void Can_Map_RelationType_Property()
        {
            // Act
            string column = new RelationMapper(new SqlCeSyntaxProvider()).Map("RelationTypeId");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoRelation].[relType]"));
        }
    }
}