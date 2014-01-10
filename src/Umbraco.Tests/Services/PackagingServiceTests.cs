using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.Services.Importing;
using Umbraco.Tests.TestHelpers.Entities;

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
            CreateTestData();
            var dictionaryItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Parent");

            var newPackageXml = XElement.Parse(ImportResources.Dictionary_Package);
            var dictionaryItemsElement = newPackageXml.Elements("DictionaryItems").First();

            // Act
            var xml = ServiceContext.PackagingService.Export(new []{dictionaryItem});

            // Assert
            Assert.That(xml.ToString(), Is.EqualTo(dictionaryItemsElement.ToString()));
        }

        public void CreateTestData()
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