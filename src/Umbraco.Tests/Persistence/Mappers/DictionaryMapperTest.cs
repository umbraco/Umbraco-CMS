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

            // Act
            string column = new DictionaryMapper().Map(new SqlCeSyntaxProvider(), "Id");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDictionary].[pk]"));
        }

        [Test]
        public void Can_Map_Key_Property()
        {

            // Act
            string column = new DictionaryMapper().Map(new SqlCeSyntaxProvider(), "Key");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDictionary].[id]"));
        }

        [Test]
        public void Can_Map_ItemKey_Property()
        {

            // Act
            string column = new DictionaryMapper().Map(new SqlCeSyntaxProvider(), "ItemKey");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsDictionary].[key]"));
        }
    }
}
