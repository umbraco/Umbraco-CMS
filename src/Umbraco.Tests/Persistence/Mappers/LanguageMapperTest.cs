using NUnit.Framework;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Persistence.Mappers
{
    [TestFixture]
    public class LanguageMapperTest : MapperTestBase
    {
        [Test]
        public void Can_Map_Id_Property()
        {

            // Act
            string column = new LanguageMapper(MockSqlContext(), CreateMaps()).Map("Id");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoLanguage].[id]"));
        }

        [Test]
        public void Can_Map_IsoCode_Property()
        {

            // Act
            string column = new LanguageMapper(MockSqlContext(), CreateMaps()).Map("IsoCode");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoLanguage].[languageISOCode]"));
        }

        [Test]
        public void Can_Map_CultureName_Property()
        {
            // Act
            string column = new LanguageMapper(MockSqlContext(), CreateMaps()).Map("CultureName");

            // Assert
            Assert.That(column, Is.EqualTo("[umbracoLanguage].[languageCultureName]"));
        }
    }
}
