// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DictionaryRepositoryTest : UmbracoIntegrationTest
{
    [SetUp]
    public async Task SetUp() => await CreateTestData();

    private IDictionaryRepository CreateRepository() => GetRequiredService<IDictionaryRepository>();

    private IDictionaryRepository CreateRepositoryWithCache(AppCaches cache, bool enableValueSearch = false)
    {
        var dictionarySettingsMonitor = new Mock<IOptionsMonitor<DictionarySettings>>();
        dictionarySettingsMonitor.Setup(x => x.CurrentValue).Returns(new DictionarySettings { EnableValueSearch = enableValueSearch });

        // Create a repository with a real runtime cache.
        return new DictionaryRepository(
            GetRequiredService<IScopeAccessor>(),
            cache,
            GetRequiredService<ILogger<DictionaryRepository>>(),
            GetRequiredService<ILoggerFactory>(),
            GetRequiredService<ILanguageRepository>(),
            GetRequiredService<IRepositoryCacheVersionService>(),
            GetRequiredService<ICacheSyncService>(),
            dictionarySettingsMonitor.Object);
    }

    [Test]
    public async Task Can_Perform_Get_By_Key_On_DictionaryRepository()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
            var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(await languageService.GetAsync("en-US"), "Hello world")
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
    public async Task Can_Perform_Get_By_UniqueId_On_DictionaryRepository()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
            var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(await languageService.GetAsync("en-US"), "Hello world")
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
    public async Task Can_Perform_Get_On_DictionaryRepository()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();
            var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(await languageService.GetAsync("en-US"), "Hello world")
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
    public async Task Can_Perform_Update_WithNewTranslation_On_DictionaryRepository()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();
        var provider = ScopeProvider;
        using (provider.CreateScope())
        {
            var repository = CreateRepository();

            var languageNo = new Language("nb-NO", "Norwegian Bokmål (Norway)");
            await languageService.CreateAsync(languageNo, Constants.Security.SuperUserKey);

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
            Assert.That(dictionaryItem.Translations.Single(t => t.LanguageIsoCode == languageNo.IsoCode).Value, Is.EqualTo("Les mer"));
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

    [Test]
    public void Can_Perform_Cached_Request_For_Existing_Value_By_Key_On_DictionaryRepository_With_Cache()
    {
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache);

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More");

            Assert.AreEqual("Read More", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }

        // Modify the value directly in the database. This won't be reflected in the repository cache and hence if the cache
        // is working as expected we should get the same value as above.
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.Database.Execute("UPDATE cmsLanguageText SET value = 'Read More (updated)' WHERE value = 'Read More' and LanguageId = 1");
            scope.Complete();
        }

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More");

            Assert.AreEqual("Read More", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }

        cache.IsolatedCaches.ClearCache<IDictionaryItem>();
        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More");

            Assert.AreEqual("Read More (updated)", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }
    }

    [Test]
    public void Can_Perform_Cached_Request_For_NonExisting_Value_By_Key_On_DictionaryRepository_With_Cache()
    {
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache);

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More Updated");

            Assert.IsNull(dictionaryItem);
        }

        // Modify the value directly in the database such that it now exists. This won't be reflected in the repository cache and hence if the cache
        // is working as expected we should get the same null value as above.
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.Database.Execute("UPDATE cmsDictionary SET [key] = 'Read More Updated' WHERE [key] = 'Read More'");
            scope.Complete();
        }

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More Updated");

            Assert.IsNull(dictionaryItem);
        }

        cache.IsolatedCaches.ClearCache<IDictionaryItem>();
        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More Updated");

            Assert.IsNotNull(dictionaryItem);
        }
    }

    [Test]
    public void Cannot_Perform_Cached_Request_For_Existing_Value_By_Key_On_DictionaryRepository_Without_Cache()
    {
        var repository = CreateRepository();

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More");

            Assert.AreEqual("Read More", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }

        // Modify the value directly in the database. As we don't have caching enabled on the repository we should get the new value.
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.Database.Execute("UPDATE cmsLanguageText SET value = 'Read More (updated)' WHERE value = 'Read More' and LanguageId = 1");
            scope.Complete();
        }

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More");

            Assert.AreEqual("Read More (updated)", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }
    }

    [Test]
    public void Cannot_Perform_Cached_Request_For_NonExisting_Value_By_Key_On_DictionaryRepository_Without_Cache()
    {
        var repository = CreateRepository();

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More Updated");

            Assert.IsNull(dictionaryItem);
        }

        // Modify the value directly in the database such that it now exists. As we don't have caching enabled on the repository we should get the new value.
        using (var scope = ScopeProvider.CreateScope())
        {
            scope.Database.Execute("UPDATE cmsDictionary SET [key] = 'Read More Updated' WHERE [key] = 'Read More'");
            scope.Complete();
        }

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More Updated");

            Assert.IsNotNull(dictionaryItem);
        }
    }

    [Test]
    public void GetDictionaryItemDescendants_WithValueSearch_Disabled_Does_Not_Return_Items_Matching_Only_Translation_Value()
    {
        // Arrange
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache, enableValueSearch: false);

        using (ScopeProvider.CreateScope())
        {
            // Act - Search for "Læs" which only exists in Danish translation value, not in any key
            var results = repository.GetDictionaryItemDescendants(null, "Læs").ToArray();

            // Assert - Should not find anything because value search is disabled
            Assert.That(results, Is.Empty);
        }
    }

    [Test]
    public void GetDictionaryItemDescendants_WithValueSearch_Enabled_Returns_Items_Matching_Translation_Value()
    {
        // Arrange
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache, enableValueSearch: true);

        using (ScopeProvider.CreateScope())
        {
            // Act - Search for "Læs" which only exists in Danish translation value, not in any key
            var results = repository.GetDictionaryItemDescendants(null, "Læs").ToArray();

            // Assert - Should find "Read More" because its Danish translation contains "Læs mere"
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].ItemKey, Is.EqualTo("Read More"));

            // - also verify that both languages have a translation
            var translatedIsoCodes = results[0]
                .Translations
                .Where(translation => translation.Value.IsNullOrWhiteSpace() == false)
                .Select(translation => translation.LanguageIsoCode)
                .ToArray();
            Assert.That(translatedIsoCodes, Does.Contain("en-US"));
            Assert.That(translatedIsoCodes, Does.Contain("da-DK"));
        }
    }

    public async Task CreateTestData()
    {
        var languageService = GetRequiredService<ILanguageService>();
        var dictionaryItemService = GetRequiredService<IDictionaryItemService>();
        var language = await languageService.GetAsync("en-US");

        var languageDK = new Language("da-DK", "Danish (Denmark)");
        await languageService.CreateAsync(languageDK, Constants.Security.SuperUserKey); //Id 2

        await dictionaryItemService.CreateAsync(
            new DictionaryItem("Read More")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(language, "Read More"), new DictionaryTranslation(languageDK, "Læs mere")
                }
            },
            Constants.Security.SuperUserKey); // Id 1

        await dictionaryItemService.CreateAsync(
            new DictionaryItem("Article")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(language, "Article"), new DictionaryTranslation(languageDK, "Artikel")
                }
            },
            Constants.Security.SuperUserKey); // Id 2
    }
}
