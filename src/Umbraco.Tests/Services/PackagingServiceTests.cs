using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Services.Importing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class PackagingServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void PackagingService_Can_Export_DictionaryItems()
        {
            // Arrange
            CreateDictionaryData();
            var dictionaryItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Parent");

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

            // Act
            var xml = ServiceContext.PackagingService.Export(new []{dictionaryItem});

            // Assert
            Assert.That(xml.ToString(), Is.EqualTo(dictionaryItemsElement.ToString()));
        }

        [Test]
        public void PackagingService_Can_Export_Languages()
        {
            // Arrange
            var languageNbNo = new Language("nb-NO") { CultureName = "Norwegian" };
            ServiceContext.LocalizationService.Save(languageNbNo);

            var languageEnGb = new Language("en-GB") { CultureName = "English (United Kingdom)" };
            ServiceContext.LocalizationService.Save(languageEnGb);

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            var languageItemsElement = newPackageXml.Elements("Languages").First();

            // Act
            var xml = ServiceContext.PackagingService.Export(new[] { languageNbNo, languageEnGb });

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