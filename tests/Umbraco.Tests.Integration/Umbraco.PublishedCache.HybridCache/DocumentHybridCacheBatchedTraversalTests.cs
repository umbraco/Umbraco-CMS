using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Full-stack coverage of the batched cold read-through: drives the real navigation traversal
/// extensions (<c>Children()</c> / <c>Descendants()</c>) against a real database with a cold
/// cache, and asserts the child set is materialised through the batched repository method rather than
/// one single-row read per item.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheBatchedTraversalTests : UmbracoIntegrationTestWithContent
{
    private const int ExtraChildCount = 4;
    private const int GrandchildCount = 3;
    private const string DefaultCulture = "en-US";

    private CountingDatabaseCacheRepository _countingRepository = null!;

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    private IDocumentNavigationQueryService NavigationQueryService => GetRequiredService<IDocumentNavigationQueryService>();

    private IPublishedContentStatusFilteringService FilteringService => GetRequiredService<IPublishedContentStatusFilteringService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        // Deliver cache refreshers synchronously in-process so the navigation and publish-status
        // structures the traversal depends on are populated on publish (the harness default is a no-op
        // messenger — see tests/Umbraco.Tests.Integration/CLAUDE.md).
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();

        // A deterministic variation-context accessor whose set culture persists (the default
        // HttpContext-backed accessor does not without a request), so the traversal resolves a culture.
        builder.Services.AddUnique<IVariationContextAccessor, TestVariationContextAccessor>();

        // Disable seeding so a cleared cache stays genuinely cold for the tree under test.
        builder.Services.PostConfigure<CacheSettings>(options =>
        {
            options.DocumentBreadthFirstSeedCount = 0;
            options.MediaBreadthFirstSeedCount = 0;
        });

        // Wrap the real database cache repository so single vs batched reads can be counted.
        builder.Services.AddUnique<IDatabaseCacheRepository>(sp =>
            new CountingDatabaseCacheRepository(ActivatorUtilities.CreateInstance<DatabaseCacheRepository>(sp)));
    }

    public override void Setup()
    {
        base.Setup();

        // The base fixture gives Textpage three children (Subpage, Subpage2, Subpage3). Add more so the
        // traversal spans several of the enumerator's growing chunks, plus grandchildren under Subpage
        // for the descendants case.
        for (var i = 0; i < ExtraChildCount; i++)
        {
            Content child = ContentBuilder.CreateSimpleContent(ContentType, $"Extra Child {i}", Textpage.Id);
            ContentService.Save(child, -1);
        }

        for (var i = 0; i < GrandchildCount; i++)
        {
            Content grandchild = ContentBuilder.CreateSimpleContent(ContentType, $"Grandchild {i}", Subpage.Id);
            ContentService.Save(grandchild, -1);
        }
    }

    [Test]
    public async Task Can_Materialise_Cold_Children_Via_Batched_Read()
    {
        const int expectedChildCount = 3 + ExtraChildCount; // base subpages + the extras added above.

        IPublishedContent root = await PublishBranchAndGoColdThenGetRoot();

        List<IPublishedContent> children = root.Children(NavigationQueryService, FilteringService).ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedChildCount, children.Count, "All published children should be returned.");
            Assert.AreEqual(0, _countingRepository.SingleContentReads, "A cold traversal must not read one row per item.");
            Assert.GreaterOrEqual(_countingRepository.BatchContentReads, 1, "A cold traversal must issue at least one batched read.");
            AssertOrderedBySortOrder(children);
        });
    }

    [Test]
    public async Task Can_Materialise_Cold_Descendants_Via_Batched_Read()
    {
        const int expectedDescendantCount = 3 + ExtraChildCount + GrandchildCount; // children + grandchildren.

        IPublishedContent root = await PublishBranchAndGoColdThenGetRoot();

        List<IPublishedContent> descendants = root.Descendants(NavigationQueryService, FilteringService).ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedDescendantCount, descendants.Count, "All published descendants should be returned.");
            Assert.AreEqual(0, _countingRepository.SingleContentReads, "A cold traversal must not read one row per item.");
            Assert.GreaterOrEqual(_countingRepository.BatchContentReads, 1, "A cold traversal must issue at least one batched read.");
        });
    }

    // Publishes the whole Textpage branch, empties both cache tiers (seeding disabled so it stays cold),
    // resolves the root, then resets the read counters — so only the traversal under test is measured.
    private async Task<IPublishedContent> PublishBranchAndGoColdThenGetRoot()
    {
        _countingRepository = (CountingDatabaseCacheRepository)GetRequiredService<IDatabaseCacheRepository>();

        ContentService.PublishBranch(Textpage, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Populate the publish-status service the filtering service consults (in the running app the
        // content cache refresher does this on publish; the integration harness doesn't run it).
        await GetRequiredService<IPublishStatusManagementService>()
            .AddOrUpdateStatusWithDescendantsAsync(Textpage.Key, CancellationToken.None);

        var cacheService = (DocumentCacheService)DocumentCacheService;
        cacheService.ResetSeedKeys();
        await DocumentCacheService.ClearMemoryCacheAsync(CancellationToken.None);

        // Set the variation context as a real request would; the traversal resolves its culture from
        // here, and the invariant content under test is published under the default culture.
        GetRequiredService<IVariationContextAccessor>().VariationContext = new VariationContext(DefaultCulture);

        IPublishedContent? root = await PublishedContentCache.GetByIdAsync(Textpage.Key);
        Assert.IsNotNull(root);

        // Sanity-check the precondition: the children are genuinely cold before the traversal runs, so
        // that a passing test cannot be a false green from a warm cache.
        Assert.IsTrue(
            NavigationQueryService.TryGetChildrenKeys(Textpage.Key, out IEnumerable<Guid> childKeys),
            "Navigation should know the children.");
        foreach (Guid childKey in childKeys)
        {
            Assert.IsFalse(
                DocumentCacheService.TryGetCached(childKey, false, out _),
                "Children must not be in the converted-content (L0) cache before the cold traversal.");
        }

        _countingRepository.Reset();
        return root!;
    }

    private static void AssertOrderedBySortOrder(IReadOnlyList<IPublishedContent> items)
    {
        for (var i = 1; i < items.Count; i++)
        {
            Assert.GreaterOrEqual(
                items[i].SortOrder,
                items[i - 1].SortOrder,
                "Children should be returned in sort order.");
        }
    }

    /// <summary>
    /// Decorates the real <see cref="IDatabaseCacheRepository"/>, forwarding every call while counting
    /// single-row versus batched reads. The batched methods must be overridden explicitly: the interface
    /// default loops the single-row method, so without these overrides the decorator would never reach
    /// the real batched query.
    /// </summary>
    private sealed class CountingDatabaseCacheRepository : IDatabaseCacheRepository
    {
        private readonly IDatabaseCacheRepository _inner;

        public CountingDatabaseCacheRepository(IDatabaseCacheRepository inner) => _inner = inner;

        public int SingleContentReads { get; private set; }

        public int BatchContentReads { get; private set; }

        public int SingleMediaReads { get; private set; }

        public int BatchMediaReads { get; private set; }

        public void Reset()
        {
            SingleContentReads = 0;
            BatchContentReads = 0;
            SingleMediaReads = 0;
            BatchMediaReads = 0;
        }

        public Task<ContentCacheNode?> GetContentSourceAsync(Guid key, bool preview = false)
        {
            SingleContentReads++;
            return _inner.GetContentSourceAsync(key, preview);
        }

        public Task<IEnumerable<ContentCacheNode>> GetContentSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
        {
            BatchContentReads++;
            return _inner.GetContentSourcesAsync(keys, preview);
        }

        public Task<ContentCacheNode?> GetMediaSourceAsync(Guid key)
        {
            SingleMediaReads++;
            return _inner.GetMediaSourceAsync(key);
        }

        public Task<IEnumerable<ContentCacheNode>> GetMediaSourcesAsync(IEnumerable<Guid> keys)
        {
            BatchMediaReads++;
            return _inner.GetMediaSourcesAsync(keys);
        }

        public Task DeleteContentItemAsync(int id) => _inner.DeleteContentItemAsync(id);

        public IEnumerable<ContentCacheNode> GetContentByContentTypeKey(IEnumerable<Guid> keys, ContentCacheDataSerializerEntityType entityType)
            => _inner.GetContentByContentTypeKey(keys, entityType);

        public IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
            => _inner.GetDocumentKeysByContentTypeKeys(keys, published);

        public Task RefreshContentAsync(ContentCacheNode contentCacheNode) => _inner.RefreshContentAsync(contentCacheNode);

        public Task RefreshMediaAsync(ContentCacheNode contentCacheNode) => _inner.RefreshMediaAsync(contentCacheNode);

        public Task RemovePublishedContentAsync(int id) => _inner.RemovePublishedContentAsync(id);

        public void Rebuild(
            IReadOnlyCollection<int>? contentTypeIds,
            IReadOnlyCollection<int>? mediaTypeIds,
            IReadOnlyCollection<int>? memberTypeIds,
            Action<Action>? executeStep)
            => _inner.Rebuild(contentTypeIds, mediaTypeIds, memberTypeIds, executeStep);
    }
}
