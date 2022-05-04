// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class DictionaryRepositoryTest : UmbracoIntegrationTest
    {
        [SetUp]
        public void SetUp() => CreateTestData();

        private IDictionaryRepository CreateRepository() => GetRequiredService<IDictionaryRepository>();

        [Test]
        public void Can_Perform_Get_By_Key_On_DictionaryRepository()
        {
            // Arrange
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();
                var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
                {
                    Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(localizationService.GetLanguageByIsoCode("en-US"), "Hello world")
                    }
                };

                repository.Save(dictionaryItem);

                // re-get
                dictionaryItem = repository.Get("Testing1235");

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.True);
                Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
                Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));
            }
        }

        [Test]
        public void Can_Perform_Get_By_UniqueId_On_DictionaryRepository()
        {
            // Arrange
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();
                var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
                {
                    Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(localizationService.GetLanguageByIsoCode("en-US"), "Hello world")
                    }
                };

                repository.Save(dictionaryItem);

                // re-get
                dictionaryItem = repository.Get(dictionaryItem.Key);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.True);
                Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
                Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));
            }
        }

        [Test]
        public void Can_Perform_Get_On_DictionaryRepository()
        {
            // Arrange
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();
                var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
                {
                    Translations = new List<IDictionaryTranslation>
                    {
                        new DictionaryTranslation(localizationService.GetLanguageByIsoCode("en-US"), "Hello world")
                    }
                };

                repository.Save(dictionaryItem);

                // re-get
                dictionaryItem = repository.Get(dictionaryItem.Id);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.True);
                Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
                Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));
            }
        }

        [Test]
        public void Can_Perform_Get_On_DictionaryRepository_When_No_Language_Assigned()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();
                var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235");

                repository.Save(dictionaryItem);

                // re-get
                dictionaryItem = repository.Get(dictionaryItem.Id);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
                Assert.That(dictionaryItem.Translations.Any(), Is.False);
            }
        }

        [Test]
        public void Can_Perform_GetAll_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                // Act
                IDictionaryItem dictionaryItem = repository.Get(1);
                IEnumerable<IDictionaryItem> dictionaryItems = repository.GetMany();

                // Assert
                Assert.That(dictionaryItems, Is.Not.Null);
                Assert.That(dictionaryItems.Any(), Is.True);
                Assert.That(dictionaryItems.Any(x => x == null), Is.False);
                Assert.That(dictionaryItems.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetAll_With_Params_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                // Act
                IEnumerable<IDictionaryItem> dictionaryItems = repository.GetMany(1, 2);

                // Assert
                Assert.That(dictionaryItems, Is.Not.Null);
                Assert.That(dictionaryItems.Any(), Is.True);
                Assert.That(dictionaryItems.Any(x => x == null), Is.False);
                Assert.That(dictionaryItems.Count(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_Perform_GetByQuery_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                // Act
                IQuery<IDictionaryItem> query = provider.CreateQuery<IDictionaryItem>().Where(x => x.ItemKey == "Article");
                IEnumerable<IDictionaryItem> result = repository.Get(query);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Any(), Is.True);
                Assert.That(result.FirstOrDefault().ItemKey, Is.EqualTo("Article"));
            }
        }

        [Test]
        public void Can_Perform_Count_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                // Act
                IQuery<IDictionaryItem> query = provider.CreateQuery<IDictionaryItem>().Where(x => x.ItemKey.StartsWith("Read"));
                int result = repository.Count(query);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_Perform_Add_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                ILanguageRepository languageRepository = GetRequiredService<ILanguageRepository>();
                IDictionaryRepository repository = CreateRepository();

                ILanguage language = languageRepository.Get(1);

                var read = new DictionaryItem("Read");
                var translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(language, "Read")
                };
                read.Translations = translations;

                // Act
                repository.Save(read);

                bool exists = repository.Exists(read.Id);

                // Assert
                Assert.That(read.HasIdentity, Is.True);
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_Update_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                // Act
                IDictionaryItem item = repository.Get(1);
                var translations = item.Translations.ToList();
                translations[0].Value = "Read even more";
                item.Translations = translations;

                repository.Save(item);

                IDictionaryItem dictionaryItem = repository.Get(1);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.Translations.Count(), Is.EqualTo(2));
                Assert.That(dictionaryItem.Translations.FirstOrDefault().Value, Is.EqualTo("Read even more"));
            }
        }

        [Test]
        public void Can_Perform_Update_WithNewTranslation_On_DictionaryRepository()
        {
            // Arrange
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                var globalSettings = new GlobalSettings();
                var languageNo = new Language(globalSettings, "nb-NO") { CultureName = "nb-NO" };
                localizationService.Save(languageNo);

                // Act
                IDictionaryItem item = repository.Get(1);
                var translations = item.Translations.ToList();
                translations.Add(new DictionaryTranslation(languageNo, "Les mer"));
                item.Translations = translations;

                repository.Save(item);

                var dictionaryItem = (DictionaryItem)repository.Get(1);

                // Assert
                Assert.That(dictionaryItem, Is.Not.Null);
                Assert.That(dictionaryItem.Translations.Count(), Is.EqualTo(3));
                Assert.That(dictionaryItem.Translations.Single(t => t.LanguageId == languageNo.Id).Value, Is.EqualTo("Les mer"));
            }
        }

        [Test]
        public void Can_Perform_Delete_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                // Act
                IDictionaryItem item = repository.Get(1);
                repository.Delete(item);

                bool exists = repository.Exists(1);

                // Assert
                Assert.That(exists, Is.False);
            }
        }

        [Test]
        public void Can_Perform_Exists_On_DictionaryRepository()
        {
            // Arrange
            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();

                // Act
                bool exists = repository.Exists(1);

                // Assert
                Assert.That(exists, Is.True);
            }
        }

        [Test]
        public void Can_Perform_GetDictionaryItemKeyMap_On_DictionaryRepository()
        {
            Dictionary<string, Guid> keyMap;

            IScopeProvider provider = ScopeProvider;
            using (provider.CreateScope())
            {
                IDictionaryRepository repository = CreateRepository();
                keyMap = repository.GetDictionaryItemKeyMap();
            }

            Assert.IsNotNull(keyMap);
            Assert.IsNotEmpty(keyMap);
            foreach (KeyValuePair<string, Guid> kvp in keyMap)
            {
                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
            }
        }

        public void CreateTestData()
        {
            ILocalizationService localizationService = GetRequiredService<ILocalizationService>();
            ILanguage language = localizationService.GetLanguageByIsoCode("en-US");

            var globalSettings = new GlobalSettings();
            var languageDK = new Language(globalSettings, "da-DK") { CultureName = "da-DK" };
            localizationService.Save(languageDK); // Id 2

            var readMore = new DictionaryItem("Read More");
            var translations = new List<IDictionaryTranslation>
                                   {
                                       new DictionaryTranslation(language, "Read More"),
                                       new DictionaryTranslation(languageDK, "Læs mere")
                                   };
            readMore.Translations = translations;
            localizationService.Save(readMore); // Id 1

            var article = new DictionaryItem("Article");
            var translations2 = new List<IDictionaryTranslation>
                                   {
                                       new DictionaryTranslation(language, "Article"),
                                       new DictionaryTranslation(languageDK, "Artikel")
                                   };
            article.Translations = translations2;
            localizationService.Save(article); // Id 2
        }
    }
}
