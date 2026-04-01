// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.EntityFrameworkCore;
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
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
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
            GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
            cache,
            GetRequiredService<ILogger<DictionaryRepository>>(),
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
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();
            var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(await languageService.GetAsync("en-US"), "Hello world")
                }
            };

            await repository.SaveAsync(dictionaryItem, CancellationToken.None);

            // re-get
            dictionaryItem = await repository.GetByItemKeyAsync("Testing1235");

            // Assert
            Assert.That(dictionaryItem, Is.Not.Null);
            Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
            Assert.That(dictionaryItem.Translations.Any(), Is.True);
            Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
            Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Get_By_UniqueId_On_DictionaryRepository()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();
            var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(await languageService.GetAsync("en-US"), "Hello world")
                }
            };

            await repository.SaveAsync(dictionaryItem, CancellationToken.None);

            // re-get by Guid Key
            dictionaryItem = await repository.GetAsync(dictionaryItem.Key, CancellationToken.None);

            // Assert
            Assert.That(dictionaryItem, Is.Not.Null);
            Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
            Assert.That(dictionaryItem.Translations.Any(), Is.True);
            Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
            Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Get_On_DictionaryRepository()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();
            var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235")
            {
                Translations = new List<IDictionaryTranslation>
                {
                    new DictionaryTranslation(await languageService.GetAsync("en-US"), "Hello world")
                }
            };

            await repository.SaveAsync(dictionaryItem, CancellationToken.None);

            // re-get by Guid Key
            dictionaryItem = await repository.GetAsync(dictionaryItem.Key, CancellationToken.None);

            // Assert
            Assert.That(dictionaryItem, Is.Not.Null);
            Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
            Assert.That(dictionaryItem.Translations.Any(), Is.True);
            Assert.That(dictionaryItem.Translations.Any(x => x == null), Is.False);
            Assert.That(dictionaryItem.Translations.First().Value, Is.EqualTo("Hello world"));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Get_On_DictionaryRepository_When_No_Language_Assigned()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();
            var dictionaryItem = (IDictionaryItem)new DictionaryItem("Testing1235");

            await repository.SaveAsync(dictionaryItem, CancellationToken.None);

            // re-get by Guid Key
            dictionaryItem = await repository.GetAsync(dictionaryItem.Key, CancellationToken.None);

            // Assert
            Assert.That(dictionaryItem, Is.Not.Null);
            Assert.That(dictionaryItem.ItemKey, Is.EqualTo("Testing1235"));
            Assert.That(dictionaryItem.Translations.Any(), Is.False);

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var dictionaryItems = (await repository.GetAllAsync(CancellationToken.None)).ToArray();

            // Assert
            Assert.That(dictionaryItems, Is.Not.Null);
            Assert.That(dictionaryItems.Any(), Is.True);
            Assert.That(dictionaryItems.Any(x => x == null), Is.False);
            Assert.That(dictionaryItems.Count(), Is.EqualTo(2));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_ByKeys_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act - GetManyByItemKeysAsync with no params returns empty (only items matching the given keys)
            // To get all, use GetAllAsync or pass specific keys
            var dictionaryItems = (await repository.GetManyByItemKeysAsync("Read More", "Article")).ToArray();

            // Assert
            Assert.That(dictionaryItems, Is.Not.Null);
            Assert.That(dictionaryItems.Any(), Is.True);
            Assert.That(dictionaryItems.Any(x => x == null), Is.False);
            Assert.That(dictionaryItems.Count(), Is.EqualTo(2));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_With_Params_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Get all items to find their Guid keys
            var all = (await repository.GetAllAsync(CancellationToken.None)).ToArray();
            var keys = all.Select(x => x.Key).ToArray();

            // Act
            var dictionaryItems = (await repository.GetManyAsync(keys, CancellationToken.None)).ToArray();

            // Assert
            Assert.That(dictionaryItems, Is.Not.Null);
            Assert.That(dictionaryItems.Any(), Is.True);
            Assert.That(dictionaryItems.Any(x => x == null), Is.False);
            Assert.That(dictionaryItems.Count(), Is.EqualTo(2));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetAll_ByKeys_With_Params_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var dictionaryItems = (await repository.GetManyByItemKeysAsync("Read More", "Article")).ToArray();

            // Assert
            Assert.That(dictionaryItems, Is.Not.Null);
            Assert.That(dictionaryItems.Any(), Is.True);
            Assert.That(dictionaryItems.Any(x => x == null), Is.False);
            Assert.That(dictionaryItems.Count(), Is.EqualTo(2));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetByItemKey_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act
            var result = await repository.GetByItemKeyAsync("Article");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ItemKey, Is.EqualTo("Article"));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Add_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var languageRepository = GetRequiredService<ILanguageRepository>();
            var repository = CreateRepository();

            var language = await languageRepository.GetByIsoCodeAsync("en-US");

            var read = new DictionaryItem("Read");
            var translations = new List<IDictionaryTranslation> { new DictionaryTranslation(language, "Read") };
            read.Translations = translations;

            // Act
            await repository.SaveAsync(read, CancellationToken.None);

            var exists = await repository.ExistsAsync(read.Key, CancellationToken.None);

            // Assert
            Assert.That(read.HasIdentity, Is.True);
            Assert.That(exists, Is.True);

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Update_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act - get by string key since we no longer use int IDs
            var item = await repository.GetByItemKeyAsync("Read More");
            var translations = item.Translations.ToList();
            translations[0].Value = "Read even more";
            item.Translations = translations;

            await repository.SaveAsync(item, CancellationToken.None);

            var dictionaryItem = await repository.GetByItemKeyAsync("Read More");

            // Assert
            Assert.That(dictionaryItem, Is.Not.Null);
            Assert.That(dictionaryItem.Translations.Count(), Is.EqualTo(2));
            Assert.That(dictionaryItem.Translations.FirstOrDefault().Value, Is.EqualTo("Read even more"));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Update_WithNewTranslation_On_DictionaryRepository()
    {
        // Arrange
        var languageService = GetRequiredService<ILanguageService>();
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            var languageNo = new Language("nb-NO", "Norwegian Bokmål (Norway)");
            await languageService.CreateAsync(languageNo, Constants.Security.SuperUserKey);

            // Act - get by string key
            var item = await repository.GetByItemKeyAsync("Read More");
            var translations = item.Translations.ToList();
            translations.Add(new DictionaryTranslation(languageNo, "Les mer"));
            item.Translations = translations;

            await repository.SaveAsync(item, CancellationToken.None);

            var dictionaryItem = await repository.GetByItemKeyAsync("Read More");

            // Assert
            Assert.That(dictionaryItem, Is.Not.Null);
            Assert.That(dictionaryItem.Translations.Count(), Is.EqualTo(3));
            Assert.That(dictionaryItem.Translations.Single(t => t.LanguageIsoCode == languageNo.IsoCode).Value, Is.EqualTo("Les mer"));

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Delete_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Act - get by string key, then delete
            var item = await repository.GetByItemKeyAsync("Read More");
            Assert.That(item, Is.Not.Null);

            var itemKey = item.Key;
            await repository.DeleteAsync(item, CancellationToken.None);

            var exists = await repository.ExistsAsync(itemKey, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.False);

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Exists_On_DictionaryRepository()
    {
        // Arrange
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();

            // Get a known item to find its Guid key
            var item = await repository.GetByItemKeyAsync("Read More");
            Assert.That(item, Is.Not.Null);

            // Act
            var exists = await repository.ExistsAsync(item.Key, CancellationToken.None);

            // Assert
            Assert.That(exists, Is.True);

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_GetDictionaryItemKeyMap_On_DictionaryRepository()
    {
        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = CreateRepository();
            var keyMap = await repository.GetDictionaryItemKeyMapAsync();

            Assert.That(keyMap, Is.Not.Null);
            Assert.That(keyMap, Is.Not.Empty);
            foreach (var kvp in keyMap)
            {
                Console.WriteLine("{0}: {1}", kvp.Key, kvp.Value);
            }

            scope.Complete();
        }
    }

    [Test]
    public async Task Can_Perform_Cached_Request_For_Existing_Value_On_DictionaryRepository_With_Cache()
    {
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache);

        // First, get the Guid key for the "Read More" item so we can use GetAsync (which goes through cache policy).
        Guid readMoreKey;
        using (var scope = NewScopeProvider.CreateScope())
        {
            var item = await repository.GetByItemKeyAsync("Read More");
            Assert.That(item, Is.Not.Null);
            readMoreKey = item.Key;
            scope.Complete();
        }

        // Fetch via GetAsync (Guid) to populate the cache.
        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetAsync(readMoreKey, CancellationToken.None);

            Assert.AreEqual("Read More", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }

        // Modify the value directly in the database using EF Core raw SQL.
        // This bypasses the repository cache, so the cached value should still be returned.
        using (var scope = NewScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<UmbracoDbContext>(async db =>
            {
                await db.Database.ExecuteSqlRawAsync(
                    "UPDATE cmsLanguageText SET value = 'Read More (updated)' WHERE value = 'Read More' and LanguageId = 1");
            });
            scope.Complete();
        }

        // GetAsync should still return the cached (old) value.
        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetAsync(readMoreKey, CancellationToken.None);

            Assert.AreEqual("Read More", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }

        // After clearing the cache, the new DB value should be returned.
        cache.IsolatedCaches.ClearCache<IDictionaryItem>();
        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetAsync(readMoreKey, CancellationToken.None);

            Assert.AreEqual("Read More (updated)", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }
    }

    [Test]
    public async Task Can_Perform_Cached_Request_For_Deletion_On_DictionaryRepository_With_Cache()
    {
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache);

        // Get the Guid key for the "Read More" item and populate the cache.
        Guid readMoreKey;
        using (var scope = NewScopeProvider.CreateScope())
        {
            var item = await repository.GetAsync(
                (await repository.GetByItemKeyAsync("Read More"))!.Key,
                CancellationToken.None);
            Assert.That(item, Is.Not.Null);
            readMoreKey = item.Key;
            scope.Complete();
        }

        // Delete the item directly from the database, bypassing the repository (and cache invalidation).
        using (var scope = NewScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<UmbracoDbContext>(async db =>
            {
                await db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM cmsLanguageText WHERE UniqueId = {0}", readMoreKey);
                await db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM cmsDictionary WHERE id = {0}", readMoreKey);
            });
            scope.Complete();
        }

        // The cache should still have the item.
        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetAsync(readMoreKey, CancellationToken.None);

            Assert.IsNotNull(dictionaryItem);
            Assert.AreEqual("Read More", dictionaryItem.ItemKey);
        }

        // After clearing the cache, the item should no longer be found.
        cache.IsolatedCaches.ClearCache<IDictionaryItem>();
        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetAsync(readMoreKey, CancellationToken.None);

            Assert.IsNull(dictionaryItem);
        }
    }

    [Test]
    public async Task Cannot_Perform_Cached_Request_For_Existing_Value_By_Key_On_DictionaryRepository_Without_Cache()
    {
        var repository = CreateRepository();

        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetByItemKeyAsync("Read More");

            Assert.AreEqual("Read More", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }

        // Modify the value directly in the database. Without caching, the repository should return the new value.
        using (var scope = NewScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<UmbracoDbContext>(async db =>
            {
                await db.Database.ExecuteSqlRawAsync(
                    "UPDATE cmsLanguageText SET value = 'Read More (updated)' WHERE value = 'Read More' and LanguageId = 1");
            });
            scope.Complete();
        }

        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetByItemKeyAsync("Read More");

            Assert.AreEqual("Read More (updated)", dictionaryItem.Translations.Single(x => x.LanguageIsoCode == "en-US").Value);
        }
    }

    [Test]
    public async Task Cannot_Perform_Cached_Request_For_NonExisting_Value_By_Key_On_DictionaryRepository_Without_Cache()
    {
        var repository = CreateRepository();

        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetByItemKeyAsync("Read More Updated");

            Assert.IsNull(dictionaryItem);
        }

        // Modify the value directly in the database so it now exists. Without caching, the repository should find it.
        using (var scope = NewScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<UmbracoDbContext>(async db =>
            {
                await db.Database.ExecuteSqlRawAsync(
                    "UPDATE cmsDictionary SET [key] = 'Read More Updated' WHERE [key] = 'Read More'");
            });
            scope.Complete();
        }

        using (NewScopeProvider.CreateScope())
        {
            var dictionaryItem = await repository.GetByItemKeyAsync("Read More Updated");

            Assert.IsNotNull(dictionaryItem);
        }
    }

    [Test]
    public async Task GetDictionaryItemDescendants_WithValueSearch_Disabled_Does_Not_Return_Items_Matching_Only_Translation_Value()
    {
        // Arrange
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache, enableValueSearch: false);

        using (NewScopeProvider.CreateScope())
        {
            // Act - Search for "Læs" which only exists in Danish translation value, not in any key
            var results = (await repository.GetDictionaryItemDescendantsAsync(null, "Læs")).ToArray();

            // Assert - Should not find anything because value search is disabled
            Assert.That(results, Is.Empty);
        }
    }

    [Test]
    public async Task GetDictionaryItemDescendants_WithValueSearch_Enabled_Returns_Items_Matching_Translation_Value()
    {
        // Arrange
        var cache = AppCaches.Create(Mock.Of<IRequestCache>());
        var repository = CreateRepositoryWithCache(cache, enableValueSearch: true);

        using (NewScopeProvider.CreateScope())
        {
            // Act - Search for "Læs" which only exists in Danish translation value, not in any key
            var results = (await repository.GetDictionaryItemDescendantsAsync(null, "Læs")).ToArray();

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
        using var efCoreScope = NewScopeProvider.CreateScope();
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
        efCoreScope.Complete();
    }
}
