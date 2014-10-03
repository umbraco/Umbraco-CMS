using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering all methods in the LocalizationService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class LocalizationServiceTests : BaseServiceTest
    {
        private Guid _parentItemGuidId;
        private int _parentItemIntId;
        private Guid _childItemGuidId;
        private int _childItemIntId;
        private int _danishLangId;
        private int _englishLangId;

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
        public void LocalizationService_Can_Get_Root_Dictionary_Items()
        {
            var rootItems = ServiceContext.LocalizationService.GetRootDictionaryItems();

            Assert.NotNull(rootItems);
            Assert.IsTrue(rootItems.Any());
        }

        [Test]
        public void LocalizationService_Can_Determint_If_DictionaryItem_Exists()
        {
            var exists = ServiceContext.LocalizationService.DictionaryItemExists("Parent");
            Assert.IsTrue(exists);
        }

        [Test]
        public void LocalizationService_Can_Get_All_Languages()
        {
            var languages = ServiceContext.LocalizationService.GetAllLanguages();
            Assert.NotNull(languages);
            Assert.IsTrue(languages.Any());
            Assert.That(languages.Count(), Is.EqualTo(3));
        }

        [Test]
        public void LocalizationService_Can_Get_Dictionary_Item_By_Int_Id()
        {
            var parentItem = ServiceContext.LocalizationService.GetDictionaryItemById(_parentItemIntId);
            Assert.NotNull(parentItem);

            var childItem = ServiceContext.LocalizationService.GetDictionaryItemById(_childItemIntId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void LocalizationService_Can_Get_Dictionary_Item_By_Guid_Id()
        {
            var parentItem = ServiceContext.LocalizationService.GetDictionaryItemById(_parentItemGuidId);
            Assert.NotNull(parentItem);

            var childItem = ServiceContext.LocalizationService.GetDictionaryItemById(_childItemGuidId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void LocalizationService_Can_Get_Dictionary_Item_By_Key()
        {
            var parentItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Parent");
            Assert.NotNull(parentItem);

            var childItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(childItem);
        }

        [Test]
        public void LocalizationService_Can_Get_Dictionary_Item_Children()
        {
            var item = ServiceContext.LocalizationService.GetDictionaryItemChildren(_parentItemGuidId);
            Assert.NotNull(item);
            Assert.That(item.Count(), Is.EqualTo(1));

            foreach (var dictionaryItem in item)
            {
                Assert.IsFalse(string.IsNullOrEmpty(dictionaryItem.ItemKey));
            }
        }

        [Test]
        public void LocalizationService_Can_Get_Language_By_Culture_Code()
        {
            var danish = ServiceContext.LocalizationService.GetLanguageByCultureCode("Danish");
            var english = ServiceContext.LocalizationService.GetLanguageByCultureCode("English");
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void LocalizationService_Can_GetLanguageById()
        {
            var danish = ServiceContext.LocalizationService.GetLanguageById(_danishLangId);
            var english = ServiceContext.LocalizationService.GetLanguageById(_englishLangId);
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void LocalizationService_Can_GetLanguageByIsoCode()
        {
            var danish = ServiceContext.LocalizationService.GetLanguageByIsoCode("da-DK");
            var english = ServiceContext.LocalizationService.GetLanguageByIsoCode("en-GB");
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void LocalizationService_Does_Not_Fail_When_Language_Doesnt_Exist()
        {
            var language = ServiceContext.LocalizationService.GetLanguageByIsoCode("sv-SE");
            Assert.Null(language);
        }

        [Test]
        public void LocalizationService_Does_Not_Fail_When_DictionaryItem_Doesnt_Exist()
        {
            var item = ServiceContext.LocalizationService.GetDictionaryItemByKey("RandomKey");
            Assert.Null(item);
        }

        [Test]
        public void LocalizationService_Can_Delete_Language()
        {
            var norwegian = new Language("nb-NO") { CultureName = "Norwegian" };
            ServiceContext.LocalizationService.Save(norwegian, 0);
            Assert.That(norwegian.HasIdentity, Is.True);
            var languageId = norwegian.Id;

            ServiceContext.LocalizationService.Delete(norwegian);

            var language = ServiceContext.LocalizationService.GetLanguageById(languageId);
            Assert.Null(language);
        }

        [Test]
        public void LocalizationService_Can_Delete_DictionaryItem()
        {
            var item = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(item);

            ServiceContext.LocalizationService.Delete(item);

            var deletedItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            Assert.Null(deletedItem);
        }

        [Test]
        public void LocalizationService_Can_Update_Existing_DictionaryItem()
        {
            var item = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            foreach (var translation in item.Translations)
            {
                translation.Value = translation.Value + "UPDATED";
            }

            ServiceContext.LocalizationService.Save(item);

            var updatedItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(updatedItem);

            foreach (var translation in updatedItem.Translations)
            {
                Assert.That(translation.Value.EndsWith("UPDATED"), Is.True);
            }
        }

        public override void CreateTestData()
        {
            var danish = new Language("da-DK") { CultureName = "Danish" };
            var english = new Language("en-GB") { CultureName = "English"};
            ServiceContext.LocalizationService.Save(danish, 0);
            ServiceContext.LocalizationService.Save(english, 0);
            _danishLangId = danish.Id;
            _englishLangId = english.Id;

            var parentItem = new DictionaryItem("Parent")
            {
                Translations = new List<IDictionaryTranslation>
                               {
                                   new DictionaryTranslation(english, "ParentValue"),
                                   new DictionaryTranslation(danish, "ForældreVærdi")
                               }
            };
            ServiceContext.LocalizationService.Save(parentItem);
            _parentItemGuidId = parentItem.Key;
            _parentItemIntId = parentItem.Id;

            var childItem = new DictionaryItem(parentItem.Key, "Child")
                            {
                                Translations = new List<IDictionaryTranslation>
                                               {
                                                   new DictionaryTranslation(english, "ChildValue"),
                                                   new DictionaryTranslation(danish, "BørnVærdi")
                                               }
                            };
            ServiceContext.LocalizationService.Save(childItem);
            _childItemGuidId = childItem.Key;
            _childItemIntId = childItem.Id;
        }
        
        [Test]
        public void Find_BaseData_Language()
        {
            // Arrange
            var localizationService = ServiceContext.LocalizationService;
            
            // Act
            var languages = localizationService.GetAllLanguages();

            // Assert 
            Assert.That(3, Is.EqualTo(languages.Count()));
        }

        [Test]
        public void Save_Language_And_GetLanguageByIsoCode()
        {
            // Arrange
            var localizationService = ServiceContext.LocalizationService;
            var isoCode = "en-AU";
            var language = new Core.Models.Language(isoCode);

            // Act
            localizationService.Save(language);
            var result = localizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Save_Language_And_GetLanguageById()
        {
            var localizationService = ServiceContext.LocalizationService;
            var isoCode = "en-AU";
            var language = new Core.Models.Language(isoCode);

            // Act
            localizationService.Save(language);
            var result = localizationService.GetLanguageById(language.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Deleted_Language_Should_Not_Exist()
        {
            var localizationService = ServiceContext.LocalizationService;
            var isoCode = "en-AU";
            var language = new Core.Models.Language(isoCode);
            localizationService.Save(language);

            // Act
            localizationService.Delete(language);
            var result = localizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.Null(result);
        }

    }
}