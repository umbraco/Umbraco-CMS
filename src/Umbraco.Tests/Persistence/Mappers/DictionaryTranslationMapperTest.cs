using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class DictionaryTranslationMapperTest
    {
        [Test]
        public void Can_Map_Key_Property()
        {

            // Act
            string column = new DictionaryTranslationMapper().Map(new SqlCeSyntaxProvider(), "Key");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[UniqueId]"));
        }

        [Test]
        public void Can_Map_Language_Property()
        {

            // Act
            string column = new DictionaryTranslationMapper().Map(new SqlCeSyntaxProvider(), "Language");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[languageId]"));
        }

        [Test]
        public void Can_Map_Value_Property()
        {

            // Act
            string column = new DictionaryTranslationMapper().Map(new SqlCeSyntaxProvider(), "Value");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[value]"));
        }
    }
}
