using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class DictionaryTranslationMapperTest
    {
        [Test]
        public void Can_Map_Key_Property()
        {

            // Act
            string column = new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[UniqueId]"));
        }

        [Test]
        public void Can_Map_Language_Property()
        {

            // Act
            string column = new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Language");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[languageId]"));
        }

        [Test]
        public void Can_Map_Value_Property()
        {

            // Act
            string column = new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Value");

            // Assert
            Assert.That(column, Is.EqualTo("[cmsLanguageText].[value]"));
        }
    }
}
