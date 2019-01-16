using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.Services.Importing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class EntityXmlSerializerTests : TestWithSomeContentBase
    {
        private IEntityXmlSerializer Serializer => Factory.GetInstance<IEntityXmlSerializer>();

        [Test]
        public void Can_Export_Macro()
        {
            // Arrange
            var macro = new Macro("test1", "Test", "~/views/macropartials/test.cshtml", MacroTypes.PartialView);
            ServiceContext.MacroService.Save(macro);

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
            var dictionaryItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Parent");

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
            var languageNbNo = new Language("nb-NO") { CultureName = "Norwegian" };
            ServiceContext.LocalizationService.Save(languageNbNo);

            var languageEnGb = new Language("en-GB") { CultureName = "English (United Kingdom)" };
            ServiceContext.LocalizationService.Save(languageEnGb);

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            var languageItemsElement = newPackageXml.Elements("Languages").First();

            // Act
            var xml = Serializer.Serialize(new[] { languageNbNo, languageEnGb });

            // Assert
            Assert.That(xml.ToString(), Is.EqualTo(languageItemsElement.ToString()));
        }

        private void CreateDictionaryData()
        {
            var languageNbNo = new Language("nb-NO") { CultureName = "nb-NO" };
            ServiceContext.LocalizationService.Save(languageNbNo);

            var languageEnGb = new Language("en-GB") { CultureName = "en-GB" };
            ServiceContext.LocalizationService.Save(languageEnGb);

            var parentItem = new DictionaryItem("Parent");
            var parentTranslations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(languageNbNo, "ForelderVerdi"),
                new DictionaryTranslation(languageEnGb, "ParentValue")
            };
            parentItem.Translations = parentTranslations;
            ServiceContext.LocalizationService.Save(parentItem);

            var childItem = new DictionaryItem(parentItem.Key, "Child");
            var childTranslations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(languageNbNo, "BarnVerdi"),
                new DictionaryTranslation(languageEnGb, "ChildValue")
            };
            childItem.Translations = childTranslations;
            ServiceContext.LocalizationService.Save(childItem);
        }
    }
}