using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.Services.Importing;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
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
        public void PackagingService_Can_Export_Macro()
        {
            // Arrange
            var macro = new Macro("test1", "Test", "~/usercontrol/blah.ascx", "MyAssembly", "test.xslt", "~/views/macropartials/test.cshtml");
            ServiceContext.MacroService.Save(macro);

            // Act
            var element = ServiceContext.PackagingService.Export(macro);

            // Assert
            Assert.That(element, Is.Not.Null);
            Assert.That(element.Element("name").Value, Is.EqualTo("Test"));
            Assert.That(element.Element("alias").Value, Is.EqualTo("test1"));
            Console.Write(element.ToString());
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

        private static string GetTestPackagePath(string packageName)
        {
            const string testPackagesDirName = "Packaging\\Packages";
            string path = Path.Combine(Core.Configuration.GlobalSettings.FullpathToRoot, testPackagesDirName, packageName);
            return path;
        }


        [Test]
        public void PackagingService_Can_ImportPackage()
        {
            var packagingService = (PackagingService)ServiceContext.PackagingService;

            const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

            string testPackagePath = GetTestPackagePath(documentTypePickerUmb);

            InstallationSummary installationSummary = packagingService.InstallPackage(testPackagePath);

            Assert.IsNotNull(installationSummary);
        }


        [Test]
        public void PackagingService_Can_GetPackageMetaData()
        {
            var packagingService = (PackagingService)ServiceContext.PackagingService;

            const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

            string testPackagePath = GetTestPackagePath(documentTypePickerUmb);

            MetaData packageMetaData = packagingService.GetPackageMetaData(testPackagePath);
            Assert.IsNotNull(packageMetaData);
        }

        [Test]
        public void PackagingService_Can_GetPackageWarnings()
        {
            var packagingService = (PackagingService)ServiceContext.PackagingService;

            const string documentTypePickerUmb = "Document_Type_Picker_1.1.umb";

            string testPackagePath = GetTestPackagePath(documentTypePickerUmb);

            PreInstallWarnings preInstallWarnings = packagingService.GetPackageWarnings(testPackagePath);
            Assert.IsNotNull(preInstallWarnings);
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