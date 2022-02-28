// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    /// <summary>
    /// Tests covering all methods in the LocalizationService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class LocalizationServiceTests : UmbracoIntegrationTest
    {
        private Guid _parentItemGuidId;
        private int _parentItemIntId;
        private Guid _childItemGuidId;
        private int _childItemIntId;
        private int _danishLangId;
        private int _englishLangId;

        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

        [SetUp]
        public void SetUp() => CreateTestData();

        [Test]
        public void Can_Get_Root_Dictionary_Items()
        {
            IEnumerable<IDictionaryItem> rootItems = LocalizationService.GetRootDictionaryItems();

            Assert.NotNull(rootItems);
            Assert.IsTrue(rootItems.Any());
        }

        [Test]
        public void Can_Determint_If_DictionaryItem_Exists()
        {
            bool exists = LocalizationService.DictionaryItemExists("Parent");
            Assert.IsTrue(exists);
        }

        [Test]
        public void Can_Get_All_Languages()
        {
            IEnumerable<ILanguage> languages = LocalizationService.GetAllLanguages();
            Assert.NotNull(languages);
            Assert.IsTrue(languages.Any());
            Assert.That(languages.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Int_Id()
        {
            IDictionaryItem parentItem = LocalizationService.GetDictionaryItemById(_parentItemIntId);
            Assert.NotNull(parentItem);

            IDictionaryItem childItem = LocalizationService.GetDictionaryItemById(_childItemIntId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Guid_Id()
        {
            IDictionaryItem parentItem = LocalizationService.GetDictionaryItemById(_parentItemGuidId);
            Assert.NotNull(parentItem);

            IDictionaryItem childItem = LocalizationService.GetDictionaryItemById(_childItemGuidId);
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_By_Key()
        {
            IDictionaryItem parentItem = LocalizationService.GetDictionaryItemByKey("Parent");
            Assert.NotNull(parentItem);

            IDictionaryItem childItem = LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(childItem);
        }

        [Test]
        public void Can_Get_Dictionary_Item_Children()
        {
            IEnumerable<IDictionaryItem> item = LocalizationService.GetDictionaryItemChildren(_parentItemGuidId);
            Assert.NotNull(item);
            Assert.That(item.Count(), Is.EqualTo(1));

            foreach (IDictionaryItem dictionaryItem in item)
            {
                Assert.AreEqual(_parentItemGuidId, dictionaryItem.ParentId);
                Assert.IsFalse(string.IsNullOrEmpty(dictionaryItem.ItemKey));
            }
        }

        [Test]
        public void Can_Get_Dictionary_Item_Descendants()
        {
            using (IScope scope = ScopeProvider.CreateScope())
            {
                ILanguage en = LocalizationService.GetLanguageById(_englishLangId);
                ILanguage dk = LocalizationService.GetLanguageById(_danishLangId);

                Guid currParentId = _childItemGuidId;
                for (int i = 0; i < 25; i++)
                {
                    // Create 2 per level
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
                    LocalizationService.Save(desc1);
                    LocalizationService.Save(desc2);

                    currParentId = desc1.Key;
                }

                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlTrace = true;
                ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().EnableSqlCount = true;

                IDictionaryItem[] items = LocalizationService.GetDictionaryItemDescendants(_parentItemGuidId).ToArray();

                Debug.WriteLine("SQL CALLS: " + ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().SqlCount);

                Assert.AreEqual(51, items.Length);

                // There's a call or two to get languages, so apart from that there should only be one call per level.
                Assert.Less(ScopeAccessor.AmbientScope.Database.AsUmbracoDatabase().SqlCount, 30);
            }
        }

        [Test]
        public void Can_GetLanguageById()
        {
            ILanguage danish = LocalizationService.GetLanguageById(_danishLangId);
            ILanguage english = LocalizationService.GetLanguageById(_englishLangId);
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void Can_GetLanguageByIsoCode()
        {
            ILanguage danish = LocalizationService.GetLanguageByIsoCode("da-DK");
            ILanguage english = LocalizationService.GetLanguageByIsoCode("en-GB");
            Assert.NotNull(danish);
            Assert.NotNull(english);
        }

        [Test]
        public void Does_Not_Fail_When_Language_Doesnt_Exist()
        {
            ILanguage language = LocalizationService.GetLanguageByIsoCode("sv-SE");
            Assert.Null(language);
        }

        [Test]
        public void Does_Not_Fail_When_DictionaryItem_Doesnt_Exist()
        {
            IDictionaryItem item = LocalizationService.GetDictionaryItemByKey("RandomKey");
            Assert.Null(item);
        }

        [Test]
        public void Can_Delete_Language()
        {
            ILanguage languageNbNo = new LanguageBuilder()
                .WithCultureInfo("nb-NO")
                .Build();
            LocalizationService.Save(languageNbNo, 0);
            Assert.That(languageNbNo.HasIdentity, Is.True);
            int languageId = languageNbNo.Id;

            LocalizationService.Delete(languageNbNo);

            ILanguage language = LocalizationService.GetLanguageById(languageId);
            Assert.Null(language);
        }

        [Test]
        public void Can_Delete_Language_Used_As_Fallback()
        {
            ILanguage languageDaDk = LocalizationService.GetLanguageByIsoCode("da-DK");
            ILanguage languageNbNo = new LanguageBuilder()
                .WithCultureInfo("nb-NO")
                .WithFallbackLanguageId(languageDaDk.Id)
                .Build();
            LocalizationService.Save(languageNbNo, 0);
            int languageId = languageDaDk.Id;

            LocalizationService.Delete(languageDaDk);

            ILanguage language = LocalizationService.GetLanguageById(languageId);
            Assert.Null(language);
        }

        [Test]
        public void Can_Create_DictionaryItem_At_Root()
        {
            ILanguage english = LocalizationService.GetLanguageByIsoCode("en-US");

            var item = (IDictionaryItem)new DictionaryItem("Testing123")
            {
                Translations = new List<IDictionaryTranslation>
                               {
                                   new DictionaryTranslation(english, "Hello world")
                               }
            };
            LocalizationService.Save(item);

            // re-get
            item = LocalizationService.GetDictionaryItemById(item.Id);

            Assert.Greater(item.Id, 0);
            Assert.IsTrue(item.HasIdentity);
            Assert.IsFalse(item.ParentId.HasValue);
            Assert.AreEqual("Testing123", item.ItemKey);
            Assert.AreEqual(1, item.Translations.Count());
        }

        [Test]
        public void Can_Create_DictionaryItem_At_Root_With_Identity()
        {
            IDictionaryItem item = LocalizationService.CreateDictionaryItemWithIdentity(
                "Testing12345", null, "Hellooooo");

            // re-get
            item = LocalizationService.GetDictionaryItemById(item.Id);

            Assert.IsNotNull(item);
            Assert.Greater(item.Id, 0);
            Assert.IsTrue(item.HasIdentity);
            Assert.IsFalse(item.ParentId.HasValue);
            Assert.AreEqual("Testing12345", item.ItemKey);
            IEnumerable<ILanguage> allLangs = LocalizationService.GetAllLanguages();
            Assert.Greater(allLangs.Count(), 0);
            foreach (ILanguage language in allLangs)
            {
                Assert.AreEqual("Hellooooo", item.Translations.Single(x => x.Language.CultureName == language.CultureName).Value);
            }
        }

        [Test]
        public void Can_Add_Translation_To_Existing_Dictionary_Item()
        {
            ILanguage english = LocalizationService.GetLanguageByIsoCode("en-US");

            var item = (IDictionaryItem)new DictionaryItem("Testing123");
            LocalizationService.Save(item);

            // re-get
            item = LocalizationService.GetDictionaryItemById(item.Id);

            item.Translations = new List<IDictionaryTranslation>
            {
                new DictionaryTranslation(english, "Hello world")
            };

            LocalizationService.Save(item);

            Assert.AreEqual(1, item.Translations.Count());
            foreach (IDictionaryTranslation translation in item.Translations)
            {
                Assert.AreEqual("Hello world", translation.Value);
            }

            item.Translations = new List<IDictionaryTranslation>(item.Translations)
            {
                new DictionaryTranslation(
                    LocalizationService.GetLanguageByIsoCode("en-GB"),
                    "My new value")
            };

            LocalizationService.Save(item);

            // re-get
            item = LocalizationService.GetDictionaryItemById(item.Id);

            Assert.AreEqual(2, item.Translations.Count());
            Assert.AreEqual("Hello world", item.Translations.First().Value);
            Assert.AreEqual("My new value", item.Translations.Last().Value);
        }

        [Test]
        public void Can_Delete_DictionaryItem()
        {
            IDictionaryItem item = LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(item);

            LocalizationService.Delete(item);

            IDictionaryItem deletedItem = LocalizationService.GetDictionaryItemByKey("Child");
            Assert.Null(deletedItem);
        }

        [Test]
        public void Can_Update_Existing_DictionaryItem()
        {
            IDictionaryItem item = LocalizationService.GetDictionaryItemByKey("Child");
            foreach (IDictionaryTranslation translation in item.Translations)
            {
                translation.Value += "UPDATED";
            }

            LocalizationService.Save(item);

            IDictionaryItem updatedItem = LocalizationService.GetDictionaryItemByKey("Child");
            Assert.NotNull(updatedItem);

            foreach (IDictionaryTranslation translation in updatedItem.Translations)
            {
                Assert.That(translation.Value.EndsWith("UPDATED"), Is.True);
            }
        }

        [Test]
        public void Find_BaseData_Language()
        {
            // Act
            IEnumerable<ILanguage> languages = LocalizationService.GetAllLanguages();

            // Assert
            Assert.That(3, Is.EqualTo(languages.Count()));
        }

        [Test]
        public void Save_Language_And_GetLanguageByIsoCode()
        {
            // Arrange
            string isoCode = "en-AU";
            ILanguage languageEnAu = new LanguageBuilder()
                .WithCultureInfo(isoCode)
                .Build();

            // Act
            LocalizationService.Save(languageEnAu);
            ILanguage result = LocalizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Save_Language_And_GetLanguageById()
        {
            // Arrange
            ILanguage languageEnAu = new LanguageBuilder()
                .WithCultureInfo("en-AU")
                .Build();

            // Act
            LocalizationService.Save(languageEnAu);
            ILanguage result = LocalizationService.GetLanguageById(languageEnAu.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Set_Default_Language()
        {
            ILanguage languageEnAu = new LanguageBuilder()
                .WithCultureInfo("en-AU")
                .WithIsDefault(true)
                .Build();
            LocalizationService.Save(languageEnAu);
            ILanguage result = LocalizationService.GetLanguageById(languageEnAu.Id);

            Assert.IsTrue(result.IsDefault);

            ILanguage languageEnNz = new LanguageBuilder()
                .WithCultureInfo("en-NZ")
                .WithIsDefault(true)
                .Build();
            LocalizationService.Save(languageEnNz);
            ILanguage result2 = LocalizationService.GetLanguageById(languageEnNz.Id);

            // re-get
            result = LocalizationService.GetLanguageById(languageEnAu.Id);

            Assert.IsTrue(result2.IsDefault);
            Assert.IsFalse(result.IsDefault);
        }

        [Test]
        public void Deleted_Language_Should_Not_Exist()
        {
            string isoCode = "en-AU";
            ILanguage languageEnAu = new LanguageBuilder()
                .WithCultureInfo(isoCode)
                .Build();
            LocalizationService.Save(languageEnAu);

            // Act
            LocalizationService.Delete(languageEnAu);
            ILanguage result = LocalizationService.GetLanguageByIsoCode(isoCode);

            // Assert
            Assert.Null(result);
        }

        public void CreateTestData()
        {
            ILanguage languageDaDk = new LanguageBuilder()
                .WithCultureInfo("da-DK")
                .Build();
            ILanguage languageEnGb = new LanguageBuilder()
                .WithCultureInfo("en-GB")
                .Build();

            LocalizationService.Save(languageDaDk, 0);
            LocalizationService.Save(languageEnGb, 0);
            _danishLangId = languageDaDk.Id;
            _englishLangId = languageEnGb.Id;

            var parentItem = new DictionaryItem("Parent")
            {
                Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(languageEnGb, "ParentValue"),
                        new DictionaryTranslation(languageDaDk, "ForældreVærdi")
                    }
            };
            LocalizationService.Save(parentItem);
            _parentItemGuidId = parentItem.Key;
            _parentItemIntId = parentItem.Id;

            var childItem = new DictionaryItem(parentItem.Key, "Child")
            {
                Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(languageEnGb, "ChildValue"),
                        new DictionaryTranslation(languageDaDk, "BørnVærdi")
                    }
            };
            LocalizationService.Save(childItem);
            _childItemGuidId = childItem.Key;
            _childItemIntId = childItem.Id;
        }
    }
}
