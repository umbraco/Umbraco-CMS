// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

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
        var localizationService = GetRequiredService<ILocalizationService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
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
        var localizationService = GetRequiredService<ILocalizationService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
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
        var localizationService = GetRequiredService<ILocalizationService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var dictionaryItem = repository.Get(1);
            var dictionaryItems = repository.GetMany().ToArray();

            // Assert
            Assert.That(dictionaryItems, Is.Not.Null);
            Assert.That(dictionaryItems.Any(), Is.True);
            Assert.That(dictionaryItems.Any(x => x == null), Is.False);
            Assert.That(dictionaryItems.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public void Can_Perform_GetAll_ByKeys_On_DictionaryRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var dictionaryItems = repository.GetManyByKeys().ToArray();

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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var dictionaryItems = repository.GetMany(1, 2).ToArray();

            // Assert
            Assert.That(dictionaryItems, Is.Not.Null);
            Assert.That(dictionaryItems.Any(), Is.True);
            Assert.That(dictionaryItems.Any(x => x == null), Is.False);
            Assert.That(dictionaryItems.Count(), Is.EqualTo(2));
        }
    }

    [Test]
    public void Can_Perform_GetAll_ByKeys_With_Params_On_DictionaryRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var dictionaryItems = repository.GetManyByKeys("Read More", "Article").ToArray();

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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var query = provider.CreateQuery<IDictionaryItem>().Where(x => x.ItemKey == "Article");
            var result = repository.Get(query).ToArray();

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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var query = provider.CreateQuery<IDictionaryItem>().Where(x => x.ItemKey.StartsWith("Read"));
            var result = repository.Count(query);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }
    }

    [Test]
    public void Can_Perform_Add_On_DictionaryRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var languageRepository = GetRequiredService<ILanguageRepository>();
            var repository = CreateRepository();

            var language = languageRepository.Get(1);

            var read = new DictionaryItem("Read");
            var translations = new List<IDictionaryTranslation> { new DictionaryTranslation(language, "Read") };
            read.Translations = translations;

            // Act
            repository.Save(read);

            var exists = repository.Exists(read.Id);

            // Assert
            Assert.That(read.HasIdentity, Is.True);
            Assert.That(exists, Is.True);
        }
    }

    [Test]
    public void Can_Perform_Update_On_DictionaryRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var item = repository.Get(1);
            var translations = item.Translations.ToList();
            translations[0].Value = "Read even more";
            item.Translations = translations;

            repository.Save(item);

            var dictionaryItem = repository.Get(1);

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
        var localizationService = GetRequiredService<ILocalizationService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            var languageNo = new Language("nb-NO", "Norwegian Bokmål (Norway)");
            localizationService.Save(languageNo);

            // Act
            var item = repository.Get(1);
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
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var item = repository.Get(1);
            repository.Delete(item);

            var exists = repository.Exists(1);

            // Assert
            Assert.That(exists, Is.False);
        }
    }

    [Test]
    public void Can_Perform_Exists_On_DictionaryRepository()
    {
        // Arrange
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var exists = repository.Exists(1);

            // Assert
            Assert.That(exists, Is.True);
        }
    }

    [Test]
    public void Can_Perform_GetDictionaryItemKeyMap_On_DictionaryRepository()
    {
        Dictionary<string, Guid> keyMap;

        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
            keyMap = repository.GetDictionaryItemKeyMap();
        }

        Assert.IsNotNull(keyMap);
        Assert.IsNotEmpty(keyMap);
        foreach (var kvp in keyMap)
        {
            Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
        }
    }

    public void CreateTestData()
    {
        var localizationService = GetRequiredService<ILocalizationService>();
        var language = localizationService.GetLanguageByIsoCode("en-US");

        var languageDK = new Language("da-DK", "Danish (Denmark)");
        localizationService.Save(languageDK); //Id 2

        var readMore = new DictionaryItem("Read More");
        var translations = new List<IDictionaryTranslation>
        {
            new DictionaryTranslation(language, "Read More"), new DictionaryTranslation(languageDK, "Læs mere")
        };
        readMore.Translations = translations;
        localizationService.Save(readMore); // Id 1

        var article = new DictionaryItem("Article");
        var translations2 = new List<IDictionaryTranslation>
        {
            new DictionaryTranslation(language, "Article"), new DictionaryTranslation(languageDK, "Artikel")
        };
        article.Translations = translations2;
        localizationService.Save(article); // Id 2
    }
}
