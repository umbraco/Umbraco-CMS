using Microsoft.Extensions.DependencyInjection;
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
    private IMedia _mediaItem;

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
}
