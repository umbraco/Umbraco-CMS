using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// The media counterpart of <see cref="DocumentHybridCacheStaleSetRaceTests"/>: a read-through that read a
/// pre-refresh snapshot from the backing store must not write it back into the in-memory cache after a
/// concurrent refresh has stored a newer one. The generation guard must reject the stale write-back on both the
/// asynchronous and the synchronous read path.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MediaHybridCacheStaleSetRaceTests : UmbracoIntegrationTest
{
    private const string OldName = "Old Media";
    private const string NewName = "New Media";

    private Mock<IDatabaseCacheRepository> _databaseCacheRepository;
    private MediaCacheService _mediaCacheService;
    private Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private Media _mediaItem;
    private ContentCacheNode _oldNode;
    private ContentCacheNode _newNode;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    [SetUp]
    public void SetUpService()
    {
        IMediaType mediaType = GetRequiredService<IMediaTypeService>().Get("image")!;
        _mediaItem = new MediaBuilder()
            .WithName("Test Media Item")
            .WithMediaType(mediaType)
            .Build();
        GetRequiredService<IMediaService>().Save(_mediaItem);

        _oldNode = BuildMediaCacheNode(mediaType.Id, OldName);
        _newNode = BuildMediaCacheNode(mediaType.Id, NewName);

        _databaseCacheRepository = new Mock<IDatabaseCacheRepository>();
        _hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();

        _mediaCacheService = new MediaCacheService(
            _databaseCacheRepository.Object,
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ICoreScopeProvider>(),
            _hybridCache,
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            Enumerable.Empty<IMediaSeedKeyProvider>(),
            GetRequiredService<IPublishedModelFactory>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            new NullLogger<MediaCacheService>(),
            new ConvertedPublishedContentCacheFactory(null, new NullLogger<ConvertedPublishedContentCacheFactory>()),
            Mock.Of<IContentCacheDataSerializerFactory>());
    }

    [Test]
    public async Task Read_Through_Does_Not_Clobber_A_Concurrent_Refresh()
    {
        // The first read is the request's read-through: it reads the old snapshot and parks before writing back.
        // The second is the refresh, which reads the new snapshot.
        var readReachedDatabase = new TaskCompletionSource();
        var releaseRead = new TaskCompletionSource();
        var reads = 0;
        _databaseCacheRepository
            .Setup(x => x.GetMediaSourceAsync(_mediaItem.Key))
            .Returns(async () =>
            {
                if (Interlocked.Increment(ref reads) > 1)
                {
                    return _newNode;
                }

                readReachedDatabase.TrySetResult();
                await releaseRead.Task;
                return _oldNode;
            });

        await _hybridCache.RemoveAsync($"{_mediaItem.Key}");

        // 1. Start the request; it reads the old snapshot and parks before the cache write-back.
        Task<IPublishedContent?> staleRead = Task.Run(() => _mediaCacheService.GetByKeyAsync(_mediaItem.Key));
        await readReachedDatabase.Task;

        // 2. The refresh stores the new snapshot (and bumps the generation).
        await _mediaCacheService.RefreshMemoryCacheAsync(_mediaItem.Key);

        // 3. Let the parked request resume and perform its (now stale) write-back.
        releaseRead.TrySetResult();
        IPublishedContent? staleResult = await staleRead;
        Assert.AreEqual(OldName, staleResult?.Name, "Name should be served from the stale snapshot.");

        // 4. A fresh lookup must observe the refreshed media, not the clobbered stale snapshot.
        IPublishedContent? result = await _mediaCacheService.GetByKeyAsync(_mediaItem.Key);
        Assert.AreEqual(NewName, result?.Name, "Name was served from the clobbered stale snapshot.");
    }

    [Test]
    public async Task Synchronous_Read_Through_Does_Not_Clobber_A_Concurrent_Refresh()
    {
        // The synchronous read-through reads the old snapshot, then blocks before writing back.
        using var readReachedDatabase = new ManualResetEventSlim();
        using var releaseRead = new ManualResetEventSlim();
        _databaseCacheRepository
            .Setup(x => x.GetMediaSource(_mediaItem.Key))
            .Returns(() =>
            {
                readReachedDatabase.Set();
                releaseRead.Wait(TimeSpan.FromSeconds(10));
                return _oldNode;
            });

        // The refresh reads the new snapshot.
        _databaseCacheRepository
            .Setup(x => x.GetMediaSourceAsync(_mediaItem.Key))
            .ReturnsAsync(_newNode);

        await _hybridCache.RemoveAsync($"{_mediaItem.Key}");

        // 1. Start the synchronous request; it reads the old snapshot and blocks before the cache write-back.
        Task<IPublishedContent?> staleRead = Task.Run(() => _mediaCacheService.GetByKey(_mediaItem.Key));
        Assert.That(readReachedDatabase.Wait(TimeSpan.FromSeconds(10)), Is.True, "The read never reached the database.");

        // 2. The refresh stores the new snapshot (and bumps the generation).
        await _mediaCacheService.RefreshMemoryCacheAsync(_mediaItem.Key);

        // 3. Let the blocked request resume and perform its (now stale) write-back.
        releaseRead.Set();
        IPublishedContent? staleResult = await staleRead;
        Assert.AreEqual(OldName, staleResult?.Name, "Name should be served from the stale snapshot.");

        // 4. A fresh synchronous lookup must observe the refreshed media, not the clobbered stale snapshot.
        IPublishedContent? result = _mediaCacheService.GetByKey(_mediaItem.Key);
        Assert.AreEqual(NewName, result?.Name, "Name was served from the clobbered stale snapshot.");
        _databaseCacheRepository.Verify(x => x.GetMediaSource(_mediaItem.Key), Times.Once);
    }

    private ContentCacheNode BuildMediaCacheNode(int mediaTypeId, string name) =>
        new()
        {
            ContentTypeId = mediaTypeId,
            CreatorId = _mediaItem.CreatorId,
            CreateDate = _mediaItem.CreateDate,
            Id = _mediaItem.Id,
            Key = _mediaItem.Key,
            SortOrder = 0,
            Data = new ContentData(
                name,
                null,
                1,
                _mediaItem.UpdateDate,
                _mediaItem.CreatorId,
                -1,
                true,
                new Dictionary<string, PropertyData[]>(),
                null),
            IsDraft = false,
        };
}
