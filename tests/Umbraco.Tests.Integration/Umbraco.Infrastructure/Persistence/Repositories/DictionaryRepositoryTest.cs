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

        Assert.That(keyMap, Is.Not.Null);
        Assert.That(keyMap, Is.Not.Empty);
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

            Assert.That(dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value, Is.EqualTo("Read More"));
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

            Assert.That(dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value, Is.EqualTo("Read More"));
        }

        cache.IsolatedCaches.ClearCache<IDictionaryItem>();
        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More");

            Assert.That(dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value, Is.EqualTo("Read More (updated)"));
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

            Assert.That(dictionaryItem, Is.Null);
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

            Assert.That(dictionaryItem, Is.Null);
        }

        cache.IsolatedCaches.ClearCache<IDictionaryItem>();
        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More Updated");

            Assert.That(dictionaryItem, Is.Not.Null);
        }
    }

    [Test]
    public void Cannot_Perform_Cached_Request_For_Existing_Value_By_Key_On_DictionaryRepository_Without_Cache()
    {
        var repository = CreateRepository();

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More");

            Assert.That(dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value, Is.EqualTo("Read More"));
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

            Assert.That(dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value, Is.EqualTo("Read More (updated)"));
        }
    }

    [Test]
    public void Cannot_Perform_Cached_Request_For_NonExisting_Value_By_Key_On_DictionaryRepository_Without_Cache()
    {
        var repository = CreateRepository();

        using (ScopeProvider.CreateScope())
        {
            var dictionaryItem = repository.Get("Read More Updated");

            Assert.That(dictionaryItem, Is.Null);
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

            Assert.That(dictionaryItem, Is.Not.Null);
        }
    }

    /// <summary>
    /// Verifies that <see cref="IDictionaryRepository.GetDictionaryItemDescendants"/> returns each
    /// dictionary item exactly once when items have multiple translations.
    /// </summary>
    /// <remarks>
    /// Regression test for https://github.com/umbraco/Umbraco-CMS/issues/22640.
    /// The underlying SQL left-joins cmsDictionary with cmsLanguageText (one row per
    /// translation) and relies on NPoco's FetchOneToMany to collapse them back into a
    /// single dictionary item. FetchOneToMany only merges adjacent rows that share the
    /// same primary key, so the SQL must order rows such that translations for the same
    /// dictionary item are contiguous. Without that ordering, items with multiple
    /// translations get yielded multiple times.
    /// </remarks>
    [Test]
    public async Task GetDictionaryItemDescendants_With_Multiple_Translations_Returns_Each_Item_Exactly_Once()
    {
        var languageService = GetRequiredService<ILanguageService>();
        var languageDe = new Language("de-DE", "German (Germany)");
        await languageService.CreateAsync(languageDe, Constants.Security.SuperUserKey);
        var languageFr = new Language("fr-FR", "French (France)");
        await languageService.CreateAsync(languageFr, Constants.Security.SuperUserKey);

        var languageEn = await languageService.GetAsync("en-US");
        var languageDk = await languageService.GetAsync("da-DK");

        var dictionaryItemService = GetRequiredService<IDictionaryItemService>();
        var additionalKeys = new[] { "Header", "Footer", "Sidebar", "Title", "Subtitle" };
        foreach (var key in additionalKeys)
        {
            await dictionaryItemService.CreateAsync(
                new DictionaryItem(key)
                {
                    Translations =
                    [
                        new DictionaryTranslation(languageEn, $"{key} (en)"),
                        new DictionaryTranslation(languageDk, $"{key} (dk)"),
                        new DictionaryTranslation(languageDe, $"{key} (de)"),
                        new DictionaryTranslation(languageFr, $"{key} (fr)"),
                    ],
                },
                Constants.Security.SuperUserKey);
        }

        using (ScopeProvider.CreateScope())
        {
            var repository = CreateRepository();

            var results = repository.GetDictionaryItemDescendants(null).ToArray();

            // 2 items from CreateTestData ("Read More", "Article") + the 5 items added above.
            var expectedKeys = new[] { "Read More", "Article" }.Concat(additionalKeys).OrderBy(x => x).ToArray();
            Assert.Multiple(() =>
            {
                Assert.That(results.Select(x => x.ItemKey).OrderBy(x => x), Is.EqualTo(expectedKeys));
                Assert.That(results.Select(x => x.Key).Distinct().Count(), Is.EqualTo(results.Length), "Each dictionary item should appear exactly once.");
            });
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
