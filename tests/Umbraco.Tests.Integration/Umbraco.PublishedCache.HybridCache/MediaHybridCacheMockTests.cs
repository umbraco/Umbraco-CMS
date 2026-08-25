using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaHybridCacheMockTests : UmbracoIntegrationTest
{
    private Mock<IDatabaseCacheRepository> _mockDatabaseCacheRepository;
    private IMediaCacheService _mediaCacheService;
    private Media _mediaItem;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    [SetUp]
    public void SetUp()
    {
        IMediaType mediaType = MediaTypeService.Get("image")!;
        _mediaItem = new MediaBuilder()
            .WithName("Test Media Item")
            .WithMediaType(mediaType)
            .Build();
        MediaService.Save(_mediaItem);

        var contentData = new ContentData(
            _mediaItem.Name,
            null,
            1,
            _mediaItem.UpdateDate,
            _mediaItem.CreatorId,
            -1,
            true,
            new Dictionary<string, PropertyData[]>(),
            null);

        var mediaCacheNode = new ContentCacheNode
        {
            ContentTypeId = mediaType.Id,
            CreatorId = _mediaItem.CreatorId,
            CreateDate = _mediaItem.CreateDate,
            Id = _mediaItem.Id,
            Key = _mediaItem.Key,
            SortOrder = 0,
            Data = contentData,
            IsDraft = false,
        };

        _mockDatabaseCacheRepository = new Mock<IDatabaseCacheRepository>();
        _mockDatabaseCacheRepository
            .Setup(r => r.GetMediaSourceAsync(It.IsAny<Guid>()))
            .ReturnsAsync(mediaCacheNode);
        _mockDatabaseCacheRepository
            .Setup(r => r.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([mediaCacheNode]);

        _mediaCacheService = new MediaCacheService(
            _mockDatabaseCacheRepository.Object,
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(),
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            Enumerable.Empty<IMediaSeedKeyProvider>(),
            GetRequiredService<IPublishedModelFactory>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            new NullLogger<MediaCacheService>(),
            new ConvertedPublishedContentCacheFactory(null, new NullLogger<ConvertedPublishedContentCacheFactory>()));
    }

    [Test]
    public async Task GetByKeysAsync_BatchesDatabaseRead_AndNeverCallsSinglePerItem()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_mediaItem.Key}");

        // A set of cold keys (only the saved media item resolves against the mocked batch).
        var keys = new[] { _mediaItem.Key, Guid.NewGuid(), Guid.NewGuid() };

        IReadOnlyList<IPublishedContent> result = await _mediaCacheService.GetByKeysAsync(keys);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_mediaItem.Key, result[0].Key);

        // The single batched query is used; the per-item single query is never called.
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourceAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_PopulatesMemoryCache_SoSubsequentReadsDoNotHitDatabase()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_mediaItem.Key}");

        _ = await _mediaCacheService.GetByKeysAsync([_mediaItem.Key]);
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);

        // Now served from the in-memory (L0) cache — the sync fast path hits.
        Assert.IsTrue(_mediaCacheService.TryGetCached(_mediaItem.Key, out IPublishedContent? cached));
        Assert.IsNotNull(cached);

        // And a further retrieval makes no additional database call.
        var again = await _mediaCacheService.GetByKeyAsync(_mediaItem.Key);
        Assert.IsNotNull(again);
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourceAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_HonoursCachedNull_WithoutQueryingDatabase()
    {
        Guid missingKey = Guid.NewGuid();

        // Arranged by hand: unlike documents, the media read-through never writes a null node, so this
        // pins the probe's contract rather than a state media can reach today. The two probes are kept
        // identical deliberately, and this is what stops the media one drifting out of step.
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.SetAsync<ContentCacheNode?>($"{missingKey}", null);

        IReadOnlyList<IPublishedContent> result = await _mediaCacheService.GetByKeysAsync([missingKey]);

        Assert.IsEmpty(result);
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_EmptyInput_ReturnsEmpty()
    {
        IReadOnlyList<IPublishedContent> result = await _mediaCacheService.GetByKeysAsync(Array.Empty<Guid>());

        Assert.IsEmpty(result);
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
        _mockDatabaseCacheRepository.Verify(x => x.GetMediaSourceAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_DuplicateKeys_ResolveToSameItemAtEveryOccurrence()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_mediaItem.Key}");

        // A batched lookup preserves input multiplicity rather than collapsing duplicates, while only
        // looking the key up once against the database.
        var keys = new[] { _mediaItem.Key, _mediaItem.Key };

        IReadOnlyList<IPublishedContent> result = await _mediaCacheService.GetByKeysAsync(keys);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(_mediaItem.Key, result[0].Key);
        Assert.AreEqual(_mediaItem.Key, result[1].Key);

        _mockDatabaseCacheRepository.Verify(
            x => x.GetMediaSourcesAsync(It.Is<IEnumerable<Guid>>(k => k.Count() == 1 && k.Contains(_mediaItem.Key))),
            Times.Once);
    }

    [Test]
    public async Task GetByKeysAsync_PreservesInputOrder_AcrossMixedCacheHits()
    {
        IMediaType mediaType = MediaTypeService.Get("image")!;
        var mediaB = new MediaBuilder().WithName("Test Media Item B").WithMediaType(mediaType).Build();
        MediaService.Save(mediaB);
        var mediaC = new MediaBuilder().WithName("Test Media Item C").WithMediaType(mediaType).Build();
        MediaService.Save(mediaC);

        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_mediaItem.Key}");
        await hybridCache.RemoveAsync($"{mediaB.Key}");
        await hybridCache.RemoveAsync($"{mediaC.Key}");

        var nodesByKey = new Dictionary<Guid, ContentCacheNode>
        {
            [_mediaItem.Key] = BuildMediaCacheNode(_mediaItem, mediaType.Id),
            [mediaB.Key] = BuildMediaCacheNode(mediaB, mediaType.Id),
            [mediaC.Key] = BuildMediaCacheNode(mediaC, mediaType.Id),
        };

        _mockDatabaseCacheRepository
            .Setup(x => x.GetMediaSourceAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid key) => nodesByKey.GetValueOrDefault(key));
        _mockDatabaseCacheRepository
            .Setup(x => x.GetMediaSourcesAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync((IEnumerable<Guid> keys) => keys.Where(nodesByKey.ContainsKey).Select(k => nodesByKey[k]));

        // Warm mediaB into L0 ahead of time; the others stay cold, so the batched call below resolves a
        // mix of tiers in one go.
        _ = await _mediaCacheService.GetByKeyAsync(mediaB.Key);

        Guid[] requestOrder = [mediaC.Key, mediaB.Key, _mediaItem.Key];
        IReadOnlyList<IPublishedContent> result = await _mediaCacheService.GetByKeysAsync(requestOrder);

        CollectionAssert.AreEqual(requestOrder, result.Select(x => x.Key));
    }

    private static ContentCacheNode BuildMediaCacheNode(Media media, int mediaTypeId) =>
        new()
        {
            ContentTypeId = mediaTypeId,
            CreatorId = media.CreatorId,
            CreateDate = media.CreateDate,
            Id = media.Id,
            Key = media.Key,
            SortOrder = media.SortOrder,
            Data = new ContentData(
                media.Name,
                null,
                1,
                media.UpdateDate,
                media.CreatorId,
                -1,
                true,
                new Dictionary<string, PropertyData[]>(),
                null),
            IsDraft = false,
        };
}
