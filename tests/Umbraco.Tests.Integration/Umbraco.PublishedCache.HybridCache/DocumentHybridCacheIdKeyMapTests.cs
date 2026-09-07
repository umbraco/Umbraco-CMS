using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
///     Verifies that content passing through the published cache warms <see cref="IIdKeyMap" />, so that
///     consumers resolving between integer IDs and keys do not fall through to the database one item at a
///     time (see issue #23583).
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheIdKeyMapTests : UmbracoIntegrationTestWithContentEditing
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();

        // Wrap the real repository so the tests can assert that ID/key lookups never reach the database.
        builder.Services.AddUnique<IIdKeyMapRepository>(factory =>
            new CountingIdKeyMapRepository(
                new IdKeyMapRepository(
                    factory.GetRequiredService<global::Umbraco.Cms.Infrastructure.Scoping.IScopeAccessor>())));
    }

    private IPublishedContentCache PublishedContentHybridCache => GetRequiredService<IPublishedContentCache>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private CountingIdKeyMapRepository IdKeyMapRepository
        => (CountingIdKeyMapRepository)GetRequiredService<IIdKeyMapRepository>();

    [Test]
    public async Task Seeding_Populates_The_Id_Key_Map()
    {
        Guid key = PublishedTextPage.Key!.Value;

        // Arrange - evict the page so that seeding has something to fetch, then start from a cold map so
        // that we are observing seeding rather than an entry left behind by the test setup.
        await DocumentCacheService.RemoveFromMemoryCacheAsync(key);
        ((DocumentCacheService)DocumentCacheService).ResetSeedKeys();
        IdKeyMap.ClearCache();
        IdKeyMapRepository.Reset();

        // Act
        await DocumentCacheService.SeedAsync(CancellationToken.None);

        // Assert - the seeded node carried both identifiers, so both directions resolve from memory.
        AssertResolvesWithoutQuerying(PublishedTextPageId, key);
    }

    [Test]
    public async Task Loading_A_Document_Populates_The_Id_Key_Map()
    {
        Guid key = PublishedTextPage.Key!.Value;

        // Arrange - evict the page so the read below is a genuine cache miss, then start from a cold map.
        await DocumentCacheService.RemoveFromMemoryCacheAsync(key);
        IdKeyMap.ClearCache();
        IdKeyMapRepository.Reset();

        // Act - fetch by key, so resolving the document itself never consults the map.
        IPublishedContent? content = await PublishedContentHybridCache.GetByIdAsync(key);
        Assert.IsNotNull(content);

        // Assert
        AssertResolvesWithoutQuerying(PublishedTextPageId, key);
    }

    [Test]
    public async Task Loading_A_Document_From_The_Backing_Store_Populates_The_Id_Key_Map()
    {
        Guid key = PublishedTextPage.Key!.Value;

        // Arrange - warm the hybrid cache, then drop only the converted content cache. The read below is
        // therefore served from the backing store rather than the database, which is the common case and
        // the reason the map is populated outside the branch that reads through to the repository.
        Assert.IsNotNull(await PublishedContentHybridCache.GetByIdAsync(key));
        DocumentCacheService.ClearConvertedContentCache();
        IdKeyMap.ClearCache();
        IdKeyMapRepository.Reset();

        // Act
        Assert.IsNotNull(await PublishedContentHybridCache.GetByIdAsync(key));

        // Assert
        AssertResolvesWithoutQuerying(PublishedTextPageId, key);
    }

    private void AssertResolvesWithoutQuerying(int expectedId, Guid expectedKey)
    {
        Attempt<int> idAttempt = IdKeyMap.GetIdForKey(expectedKey, UmbracoObjectTypes.Document);
        Attempt<Guid> keyAttempt = IdKeyMap.GetKeyForId(expectedId, UmbracoObjectTypes.Document);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(idAttempt.Success);
            Assert.AreEqual(expectedId, idAttempt.Result);
            Assert.IsTrue(keyAttempt.Success);
            Assert.AreEqual(expectedKey, keyAttempt.Result);
            Assert.AreEqual(0, IdKeyMapRepository.Count, "Expected no ID/key lookups against the database.");
        });
    }

    /// <summary>
    ///     Counts the ID/key lookups that fall all the way through to the database.
    /// </summary>
    private sealed class CountingIdKeyMapRepository : IIdKeyMapRepository
    {
        private readonly IIdKeyMapRepository _inner;
        private int _count;

        public CountingIdKeyMapRepository(IIdKeyMapRepository inner) => _inner = inner;

        public int Count => Volatile.Read(ref _count);

        public void Reset() => Interlocked.Exchange(ref _count, 0);

        public int? GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
        {
            Interlocked.Increment(ref _count);
            return _inner.GetIdForKey(key, umbracoObjectType);
        }

        public Guid? GetIdForKey(int id, UmbracoObjectTypes umbracoObjectType)
        {
            Interlocked.Increment(ref _count);
            return _inner.GetIdForKey(id, umbracoObjectType);
        }
    }
}
