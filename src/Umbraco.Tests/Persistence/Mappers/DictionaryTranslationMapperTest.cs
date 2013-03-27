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
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = DictionaryTranslationMapper.Instance.Map("Key");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[UniqueId]"));
        }

        [Test]
        public void Can_Map_Language_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = DictionaryTranslationMapper.Instance.Map("Language");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[languageId]"));
        }

        [Test]
        public void Can_Map_Value_Property()
        {
            // Arrange
            SqlSyntaxContext.SqlSyntaxProvider = SqlCeSyntax.Provider;

            // Act
            string column = DictionaryTranslationMapper.Instance.Map("Value");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[value]"));
        }
    }
}