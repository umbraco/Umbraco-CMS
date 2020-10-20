using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Integration.Umbraco.Infrastructure.Services.Importing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class EntityXmlSerializerTests : UmbracoIntegrationTest
    {
        private IEntityXmlSerializer Serializer => GetRequiredService<IEntityXmlSerializer>();

        [Test]
        public void Can_Export_Macro()
        {
            // Arrange
            var macroService = GetRequiredService<IMacroService>();
            var macro = new MacroBuilder()
                .WithAlias("test1")
                .WithName("Test")
                .Build();
            macroService.Save(macro);

            // Act
            var element = Serializer.Serialize(macro);

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Element("name").Value, Is.EqualTo("Test"));
            Assert.That(element.Element("alias").Value, Is.EqualTo("test1"));
            Debug.Print(element.ToString());
        }

        [Test]
        public void Can_Export_DictionaryItems()
        {
            // Arrange
            CreateDictionaryData();
            var localizationService = GetRequiredService<ILocalizationService>();
            var dictionaryItem = localizationService.GetDictionaryItemByKey("Parent");

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

            // Act
            var xml = Serializer.Serialize(new[] { dictionaryItem });

            // Assert
            Assert.That(xml.ToString(), Is.EqualTo(dictionaryItemsElement.ToString()));
        }

        [Test]
        public void Can_Export_Languages()
        {
            // Arrange
            var localizationService = GetRequiredService<ILocalizationService>();

            var languageNbNo = new LanguageBuilder()
                .WithCultureInfo("nb-NO")
                .WithCultureName("Norwegian")
                .Build();
            localizationService.Save(languageNbNo);

            var languageEnGb = new LanguageBuilder()
                .WithCultureInfo("en-GB")
                .Build();
            localizationService.Save(languageEnGb);

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            var languageItemsElement = newPackageXml.Elements("Languages").First();

            // Act
            var xml = Serializer.Serialize(new[] { languageNbNo, languageEnGb });

            // Assert
            Assert.That(xml.ToString(), Is.EqualTo(languageItemsElement.ToString()));
        }

        private void CreateDictionaryData()
        {
            var localizationService = GetRequiredService<ILocalizationService>();

            var languageNbNo = new LanguageBuilder()
                .WithCultureInfo("nb-NO")
                .WithCultureName("Norwegian")
                .Build();
            localizationService.Save(languageNbNo);

            var languageEnGb = new LanguageBuilder()
                .WithCultureInfo("en-GB")
                .Build();
            localizationService.Save(languageEnGb);

            var parentItem = new DictionaryItem("Parent");
            var parentTranslations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(languageNbNo, "ForelderVerdi"),
                new DictionaryTranslation(languageEnGb, "ParentValue")
            };
            parentItem.Translations = parentTranslations;
            localizationService.Save(parentItem);

            var childItem = new DictionaryItem(parentItem.Key, "Child");
            var childTranslations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(languageNbNo, "BarnVerdi"),
                new DictionaryTranslation(languageEnGb, "ChildValue")
            };
            childItem.Translations = childTranslations;
            localizationService.Save(childItem);
        }
    }
}
