using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering all methods in the LocalizationService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class LocalizationServiceTests : TestWithSomeContentBase
    {
        private Guid _parentItemGuidId;
        private int _parentItemIntId;
        private Guid _childItemGuidId;
        private int _childItemIntId;
        private int _danishLangId;
        private int _englishLangId;

        [Test]
        public void Can_Get_Root_Dictionary_Items()
        {
            var rootItems = ServiceContext.LocalizationService.GetRootDictionaryItems();

            Assert.NotNull(rootItems);
            Assert.IsTrue(rootItems.Any());
        }

        [Test]
        public void Can_Determint_If_DictionaryItem_Exists()
        {
            var exists = ServiceContext.LocalizationService.DictionaryItemExists("Parent");
            Assert.IsTrue(exists);
        }

        [Test]
        public void Can_Get_All_Languages()
        {
            var languages = ServiceContext.LocalizationService.GetAllLanguages();
            Assert.NotNull(languages);
            Assert.IsTrue(languages.Any());
            Assert.That(languages.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Int_Id()
        {
            var parentItem = ServiceContext.LocalizationService.GetDictionaryItemById(_parentItemIntId);
            Assert.NotNull(parentItem);

            var childItem = ServiceContext.LocalizationService.GetDictionaryItemById(_childItemIntId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Guid_Id()
        {
            var parentItem = ServiceContext.LocalizationService.GetDictionaryItemById(_parentItemGuidId);
            Assert.NotNull(parentItem);

            var childItem = ServiceContext.LocalizationService.GetDictionaryItemById(_childItemGuidId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Key()
        {
            var parentItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Parent");
            Assert.NotNull(parentItem);

            var childItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_Children()
        {
            var item = ServiceContext.LocalizationService.GetDictionaryItemChildren(_parentItemGuidId);
            Assert.NotNull(item);
            Assert.That(item.Count(), Is.EqualTo(1));

            foreach (var dictionaryItem in item)
            {
                Assert.AreEqual(_parentItemGuidId, dictionaryItem.ParentId);
                Assert.IsFalse(string.IsNullOrEmpty(dictionaryItem.ItemKey));
            }
        }

        [Test]
        public void Can_Get_Dictionary_Item_Descendants()
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                var en = ServiceContext.LocalizationService.GetLanguageById(_englishLangId);
                var dk = ServiceContext.LocalizationService.GetLanguageById(_danishLangId);

                var currParentId = _childItemGuidId;
                for (int i = 0; i < 25; i++)
                {
                    //Create 2 per level
                    var desc1 = new DictionaryItem(currParentId, "D1" + i)
                    {
                        Translations = new List<IDictionaryTranslation>
                        {
                            new DictionaryTranslation(en, "ChildValue1 " + i),
                            new DictionaryTranslation(dk, "BørnVærdi1 " + i)
                        }
                    };
                    var desc2 = new DictionaryItem(currParentId, "D2" + i)
                    {
                        Translations = new List<IDictionaryTranslation>
                        {
                            new DictionaryTranslation(en, "ChildValue2 " + i),
                            new DictionaryTranslation(dk, "BørnVærdi2 " + i)
                        }
                    };
                    ServiceContext.LocalizationService.Save(desc1);
                    ServiceContext.LocalizationService.Save(desc2);

                    currParentId = desc1.Key;
                }

                scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                scope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                var items = ServiceContext.LocalizationService.GetDictionaryItemDescendants(_parentItemGuidId)
                    .ToArray();

                Debug.WriteLine("SQL CALLS: " + scope.Database.AsUmbracoDatabase().SqlCount);

                Assert.AreEqual(51, items.Length);
                //there's a call or two to get languages, so apart from that there should only be one call per level
                Assert.Less(scope.Database.AsUmbracoDatabase().SqlCount, 30);
            }
        }

        [Test]
        public void Can_GetLanguageById()
        {
            var danish = ServiceContext.LocalizationService.GetLanguageById(_danishLangId);
            var english = ServiceContext.LocalizationService.GetLanguageById(_englishLangId);
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void Can_GetLanguageByIsoCode()
        {
            var danish = ServiceContext.LocalizationService.GetLanguageByIsoCode("da-DK");
            var english = ServiceContext.LocalizationService.GetLanguageByIsoCode("en-GB");
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void Does_Not_Fail_When_Language_Doesnt_Exist()
        {
            var language = ServiceContext.LocalizationService.GetLanguageByIsoCode("sv-SE");
            Assert.Null(language);
        }

        [Test]
        public void Does_Not_Fail_When_DictionaryItem_Doesnt_Exist()
        {
            var item = ServiceContext.LocalizationService.GetDictionaryItemByKey("RandomKey");
            Assert.Null(item);
        }

        [Test]
        public void Can_Delete_Language()
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
        public void Can_Delete_Language_Used_As_Fallback()
        {
            var danish = ServiceContext.LocalizationService.GetLanguageByIsoCode("da-DK");
            var norwegian = new Language("nb-NO") { CultureName = "Norwegian", FallbackLanguageId = danish.Id };
            ServiceContext.LocalizationService.Save(norwegian, 0);
            var languageId = danish.Id;

            ServiceContext.LocalizationService.Delete(danish);

            var language = ServiceContext.LocalizationService.GetLanguageById(languageId);
            Assert.Null(language);
        }

        [Test]
        public void Can_Create_DictionaryItem_At_Root()
        {
            var english = ServiceContext.LocalizationService.GetLanguageByIsoCode("en-US");

            var item = (IDictionaryItem)new DictionaryItem("Testing123")
            {
                Translations = new List<IDictionaryTranslation>
                               {
                                   new DictionaryTranslation(english, "Hello world")
                               }
            };
            ServiceContext.LocalizationService.Save(item);

            //re-get
            item = ServiceContext.LocalizationService.GetDictionaryItemById(item.Id);

            Assert.Greater(item.Id, 0);
            Assert.IsTrue(item.HasIdentity);
            Assert.IsFalse(item.ParentId.HasValue);
            Assert.AreEqual("Testing123", item.ItemKey);
            Assert.AreEqual(1, item.Translations.Count());
        }

        [Test]
        public void Can_Create_DictionaryItem_At_Root_With_Identity()
        {

            var item = ServiceContext.LocalizationService.CreateDictionaryItemWithIdentity(
                "Testing12345", null, "Hellooooo");

            //re-get
            item = ServiceContext.LocalizationService.GetDictionaryItemById(item.Id);

            Assert.IsNotNull(item);
            Assert.Greater(item.Id, 0);
            Assert.IsTrue(item.HasIdentity);
            Assert.IsFalse(item.ParentId.HasValue);
            Assert.AreEqual("Testing12345", item.ItemKey);
            var allLangs = ServiceContext.LocalizationService.GetAllLanguages();
            Assert.Greater(allLangs.Count(), 0);
            foreach (var language in allLangs)
            {
                Assert.AreEqual("Hellooooo", item.Translations.Single(x => x.Language.CultureName == language.CultureName).Value);
            }

        }

        [Test]
        public void Can_Add_Translation_To_Existing_Dictionary_Item()
        {
            var english = ServiceContext.LocalizationService.GetLanguageByIsoCode("en-US");

            var item = (IDictionaryItem) new DictionaryItem("Testing123");
            ServiceContext.LocalizationService.Save(item);

            //re-get
            item = ServiceContext.LocalizationService.GetDictionaryItemById(item.Id);

            item.Translations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(english, "Hello world")
            };

            ServiceContext.LocalizationService.Save(item);

            Assert.AreEqual(1, item.Translations.Count());
            foreach (var translation in item.Translations)
            {
                Assert.AreEqual("Hello world", translation.Value);
            }

            item.Translations = new List<IDictionaryTranslation>(item.Translations)
            {
                new DictionaryTranslation(
                    ServiceContext.LocalizationService.GetLanguageByIsoCode("en-GB"),
                    "My new value")
            };

            ServiceContext.LocalizationService.Save(item);

            //re-get
            item = ServiceContext.LocalizationService.GetDictionaryItemById(item.Id);

            Assert.AreEqual(2, item.Translations.Count());
            Assert.AreEqual("Hello world", item.Translations.First().Value);
            Assert.AreEqual("My new value", item.Translations.Last().Value);
        }

        [Test]
        public void Can_Delete_DictionaryItem()
        {
            var item = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(item);

            ServiceContext.LocalizationService.Delete(item);

            var deletedItem = ServiceContext.LocalizationService.GetDictionaryItemByKey("Child");
            Assert.Null(deletedItem);
        }

        [Test]
        public void Can_Update_Existing_DictionaryItem()
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
        public void Set_Default_Language()
        {
            var localizationService = ServiceContext.LocalizationService;
            var language = new Core.Models.Language("en-AU");
            language.IsDefault = true;
            localizationService.Save(language);
            var result = localizationService.GetLanguageById(language.Id);

            Assert.IsTrue(result.IsDefault);

            var language2 = new Core.Models.Language("en-NZ");
            language2.IsDefault = true;
            localizationService.Save(language2);
            var result2 = localizationService.GetLanguageById(language2.Id);
            //re-get
            result = localizationService.GetLanguageById(language.Id);

            Assert.IsTrue(result2.IsDefault);
            Assert.IsFalse(result.IsDefault);
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

        public override void CreateTestData()
        {
            var danish = new Language("da-DK") { CultureName = "Danish" };
            var english = new Language("en-GB") { CultureName = "English" };
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

    }
}
