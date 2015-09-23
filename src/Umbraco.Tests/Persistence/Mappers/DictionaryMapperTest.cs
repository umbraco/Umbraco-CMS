using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class DictionaryMapperTest
    {
        [Test]
        public void Can_Map_Id_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new DictionaryMapper().Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDictionary].[pk]"));
        }

        [Test]
        public void Can_Map_Key_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new DictionaryMapper().Map("Key");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDictionary].[id]"));
        }

        [Test]
        public void Can_Map_ItemKey_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = new SqlCeSyntaxProvider();

            // Act
            string column = new DictionaryMapper().Map("ItemKey");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDictionary].[key]"));
        }
    }
}