using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Services
{
    /// <summary>
    /// Tests covering all methods in the LocalizationService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class LocalizationServiceTests : UmbracoIntegrationTest
    {
        private Guid _parentItemGuidId;
        private int _parentItemIntId;
        private Guid _childItemGuidId;
        private int _childItemIntId;
        private int _danishLangId;
        private int _englishLangId;

        private GlobalSettings _globalSettings;
        private ILocalizationService _localizationService => GetRequiredService<ILocalizationService>();

        [SetUp]
        public void SetUp()
        {
            _globalSettings = new GlobalSettings();
            CreateTestData();
        }

        [Test]
        public void Can_Get_Root_Dictionary_Items()
        {
            var rootItems = _localizationService.GetRootDictionaryItems();

            Assert.NotNull(rootItems);
            Assert.IsTrue(rootItems.Any());
        }

        [Test]
        public void Can_Determint_If_DictionaryItem_Exists()
        {
            var exists = _localizationService.DictionaryItemExists("Parent");
            Assert.IsTrue(exists);
        }

        [Test]
        public void Can_Get_All_Languages()
        {
            var languages = _localizationService.GetAllLanguages();
            Assert.NotNull(languages);
            Assert.IsTrue(languages.Any());
            Assert.That(languages.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Int_Id()
        {
            var parentItem = _localizationService.GetDictionaryItemById(_parentItemIntId);
            Assert.NotNull(parentItem);

            var childItem = _localizationService.GetDictionaryItemById(_childItemIntId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Guid_Id()
        {
            var parentItem = _localizationService.GetDictionaryItemById(_parentItemGuidId);
            Assert.NotNull(parentItem);

            var childItem = _localizationService.GetDictionaryItemById(_childItemGuidId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Key()
        {
            var parentItem = _localizationService.GetDictionaryItemByKey("Parent");
            Assert.NotNull(parentItem);

            var childItem = _localizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_Children()
        {
            var item = _localizationService.GetDictionaryItemChildren(_parentItemGuidId);
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
                var en = _localizationService.GetLanguageById(_englishLangId);
                var dk = _localizationService.GetLanguageById(_danishLangId);

                var currParentId = _childItemGuidId;
                for (var i = 0; i < 25; i++)
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
                    _localizationService.Save(desc1);
                    _localizationService.Save(desc2);

                    currParentId = desc1.Key;
                }

                scope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                scope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                var items = _localizationService.GetDictionaryItemDescendants(_parentItemGuidId).ToArray();

                Debug.WriteLine("SQL CALLS: " + scope.Database.AsUmbracoDatabase().SqlCount);

                Assert.AreEqual(51, items.Length);
                //there's a call or two to get languages, so apart from that there should only be one call per level
                Assert.Less(scope.Database.AsUmbracoDatabase().SqlCount, 30);
            }
        }

        [Test]
        public void Can_GetLanguageById()
        {
            var danish = _localizationService.GetLanguageById(_danishLangId);
            var english = _localizationService.GetLanguageById(_englishLangId);
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void Can_GetLanguageByIsoCode()
        {
            var danish = _localizationService.GetLanguageByIsoCode("da-DK");
            var english = _localizationService.GetLanguageByIsoCode("en-GB");
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void Does_Not_Fail_When_Language_Doesnt_Exist()
        {
            var language = _localizationService.GetLanguageByIsoCode("sv-SE");
            Assert.Null(language);
        }

        [Test]
        public void Does_Not_Fail_When_DictionaryItem_Doesnt_Exist()
        {
            var item = _localizationService.GetDictionaryItemByKey("RandomKey");
            Assert.Null(item);
        }

        [Test]
        public void Can_Delete_Language()
        {
            var norwegian = new Language(_globalSettings, "nb-NO") { CultureName = "Norwegian" };
            _localizationService.Save(norwegian, 0);
            Assert.That(norwegian.HasIdentity, Is.True);
            var languageId = norwegian.Id;

            _localizationService.Delete(norwegian);

            var language = _localizationService.GetLanguageById(languageId);
            Assert.Null(language);
        }

        [Test]
        public void Can_Delete_Language_Used_As_Fallback()
        {
            var danish = _localizationService.GetLanguageByIsoCode("da-DK");
            var norwegian = new Language(_globalSettings, "nb-NO") { CultureName = "Norwegian", FallbackLanguageId = danish.Id };
            _localizationService.Save(norwegian, 0);
            var languageId = danish.Id;

            _localizationService.Delete(danish);

            var language = _localizationService.GetLanguageById(languageId);
            Assert.Null(language);
        }

        [Test]
        public void Can_Create_DictionaryItem_At_Root()
        {
            var english = _localizationService.GetLanguageByIsoCode("en-US");

            var item = (IDictionaryItem)new DictionaryItem("Testing123")
            {
                Translations = new List<IDictionaryTranslation>
                               {
                                   new DictionaryTranslation(english, "Hello world")
                               }
            };
            _localizationService.Save(item);

            //re-get
            item = _localizationService.GetDictionaryItemById(item.Id);

            Assert.Greater(item.Id, 0);
            Assert.IsTrue(item.HasIdentity);
            Assert.IsFalse(item.ParentId.HasValue);
            Assert.AreEqual("Testing123", item.ItemKey);
            Assert.AreEqual(1, item.Translations.Count());
        }

        [Test]
        public void Can_Create_DictionaryItem_At_Root_With_Identity()
        {
            var item = _localizationService.CreateDictionaryItemWithIdentity(
                "Testing12345", null, "Hellooooo");

            //re-get
            item = _localizationService.GetDictionaryItemById(item.Id);

            Assert.IsNotNull(item);
            Assert.Greater(item.Id, 0);
            Assert.IsTrue(item.HasIdentity);
            Assert.IsFalse(item.ParentId.HasValue);
            Assert.AreEqual("Testing12345", item.ItemKey);
            var allLangs = _localizationService.GetAllLanguages();
            Assert.Greater(allLangs.Count(), 0);
            foreach (var language in allLangs)
            {
                Assert.AreEqual("Hellooooo", item.Translations.Single(x => x.Language.CultureName == language.CultureName).Value);
            }
        }

        [Test]
        public void Can_Add_Translation_To_Existing_Dictionary_Item()
        {
            var english = _localizationService.GetLanguageByIsoCode("en-US");

            var item = (IDictionaryItem) new DictionaryItem("Testing123");
            _localizationService.Save(item);

            //re-get
            item = _localizationService.GetDictionaryItemById(item.Id);

            item.Translations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(english, "Hello world")
            };

            _localizationService.Save(item);

            Assert.AreEqual(1, item.Translations.Count());
            foreach (var translation in item.Translations)
            {
                Assert.AreEqual("Hello world", translation.Value);
            }

            item.Translations = new List<IDictionaryTranslation>(item.Translations)
            {
                new DictionaryTranslation(
                    _localizationService.GetLanguageByIsoCode("en-GB"),
                    "My new value")
            };

            _localizationService.Save(item);

            //re-get
            item = _localizationService.GetDictionaryItemById(item.Id);

            Assert.AreEqual(2, item.Translations.Count());
            Assert.AreEqual("Hello world", item.Translations.First().Value);
            Assert.AreEqual("My new value", item.Translations.Last().Value);
        }

        [Test]
        public void Can_Delete_DictionaryItem()
        {
            var item = _localizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(item);

            _localizationService.Delete(item);

            var deletedItem = _localizationService.GetDictionaryItemByKey("Child");
            Assert.Null(deletedItem);
        }

        [Test]
        public void Can_Update_Existing_DictionaryItem()
        {
            var item = _localizationService.GetDictionaryItemByKey("Child");
            foreach (var translation in item.Translations)
            {
                translation.Value = translation.Value + "UPDATED";
            }

            _localizationService.Save(item);

            var updatedItem = _localizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(updatedItem);

            foreach (var translation in updatedItem.Translations)
            {
                Assert.That(translation.Value.EndsWith("UPDATED"), Is.True);
            }
        }

        [Test]
        public void Find_BaseData_Language()
        {
            // Act
            var languages = _localizationService.GetAllLanguages();

            // Assert
            Assert.That(3, Is.EqualTo(languages.Count()));
        }

        [Test]
        public void Save_Language_And_GetLanguageByIsoCode()
        {
            // Arrange
            var isoCode = "en-AU";
            var language = new Core.Models.Language(_globalSettings, isoCode);

            // Act
            _localizationService.Save(language);
            var result = _localizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Save_Language_And_GetLanguageById()
        {
            var isoCode = "en-AU";
            var language = new Core.Models.Language(_globalSettings, isoCode);

            // Act
            _localizationService.Save(language);
            var result = _localizationService.GetLanguageById(language.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Set_Default_Language()
        {
            var language = new Language(_globalSettings, "en-AU") {IsDefault = true};
            _localizationService.Save(language);
            var result = _localizationService.GetLanguageById(language.Id);

            Assert.IsTrue(result.IsDefault);

            var language2 = new Language(_globalSettings, "en-NZ") {IsDefault = true};
            _localizationService.Save(language2);
            var result2 = _localizationService.GetLanguageById(language2.Id);
            //re-get
            result = _localizationService.GetLanguageById(language.Id);

            Assert.IsTrue(result2.IsDefault);
            Assert.IsFalse(result.IsDefault);
        }

        [Test]
        public void Deleted_Language_Should_Not_Exist()
        {
            var isoCode = "en-AU";
            var language = new Core.Models.Language(_globalSettings, isoCode);
            _localizationService.Save(language);

            // Act
            _localizationService.Delete(language);
            var result = _localizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.Null(result);
        }

        public void CreateTestData()
        {
            var danish = new Language(_globalSettings, "da-DK") { CultureName = "Danish" };
            var english = new Language(_globalSettings, "en-GB") { CultureName = "English" };
            _localizationService.Save(danish, 0);
            _localizationService.Save(english, 0);
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
            _localizationService.Save(parentItem);
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
            _localizationService.Save(childItem);
            _childItemGuidId = childItem.Key;
            _childItemIntId = childItem.Id;
        }
    }
}
