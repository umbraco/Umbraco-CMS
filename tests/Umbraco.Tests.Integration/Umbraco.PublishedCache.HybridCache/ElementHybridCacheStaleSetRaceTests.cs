using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Reproduces the read-through "stale-set clobber" race in <see cref="ElementCacheService"/>:
/// a request reads a pre-publish snapshot from the backing store and then writes it back into the
/// in-memory cache *after* a concurrent publish has already refreshed it, leaving memory permanently
/// stale (or, for Block List, empty) until a full cache clear. The generation guard must reject the
/// stale write-back so the refreshed value survives.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheStaleSetRaceTests : UmbracoIntegrationTest
{
    private const string OldName = "Old Element";
    private const string OldTitle = "Old Title";
    private const string NewName = "New Element";
    private const string NewTitle = "New Title";

    private Mock<IDatabaseCacheRepository> _databaseCacheRepository;
    private ElementCacheService _elementCacheService;
    private Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private Element _element;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private ContentCacheNode BuildPublishedNode(string name, string title)
    {
        var data = new ContentData(
            name,
            null,
            1,
            _element.UpdateDate,
            _element.CreatorId,
            -1,
            true,
            new Dictionary<string, PropertyData[]>
            {
                ["title"] = [new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = title }],
            },
            null);

        return new ContentCacheNode
        {
            ContentTypeId = _element.ContentTypeId,
            CreatorId = _element.CreatorId,
            CreateDate = _element.CreateDate,
            Id = _element.Id,
            Key = _element.Key,
            SortOrder = 0,
            Data = data,
            IsDraft = false,
        };
    }

    [SetUp]
    public async Task SetUp()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await GetRequiredService<IContentTypeService>().CreateAsync(elementType, Constants.Security.SuperUserKey);

        _element = ElementBuilder.CreateSimpleElement(elementType);
        GetRequiredService<IElementService>().Save(_element);

        _databaseCacheRepository = new Mock<IDatabaseCacheRepository>();
        _hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();

        _elementCacheService = new ElementCacheService(
            _databaseCacheRepository.Object,
            GetRequiredService<ICoreScopeProvider>(),
            _hybridCache,
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            Array.Empty<IElementSeedKeyProvider>(),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewService>(),
            GetRequiredService<IOptions<CacheSettings>>(),
            new NullLogger<ElementCacheService>(),
            GetRequiredService<IConvertedPublishedContentCacheFactory>(),
            Mock.Of<IPreviewAccessEvaluator>(evaluator => evaluator.CanPreviewElement(It.IsAny<IPublishedElement>())),
            GetRequiredService<IContentCacheDataSerializerFactory>());
    }

    [Test]
    public async Task Read_Through_Does_Not_Clobber_A_Concurrent_Refresh()
    {
        ContentCacheNode oldNode = BuildPublishedNode(OldName, OldTitle);
        ContentCacheNode newNode = BuildPublishedNode(NewName, NewTitle);

        // The first read-through reads the pre-publish (old) snapshot, then parks before writing back —
        // simulating the request suspending on an await while a publish completes.
        var readReachedDatabase = new TaskCompletionSource();
        var releaseRead = new TaskCompletionSource();
        _databaseCacheRepository
            .Setup(x => x.GetElementSourceAsync(_element.Key, false))
            .Returns(async () =>
            {
                readReachedDatabase.TrySetResult();
                await releaseRead.Task;
                return oldNode;
            });

        // The publish-time memory refresh reads the new snapshot from the database cache.
        _databaseCacheRepository
            .Setup(x => x.GetElementSourceForPublishStatesAsync(_element.Key))
            .ReturnsAsync((null, newNode));

        // Ensure the published entry is absent so the request below is a genuine read-through.
        await _hybridCache.RemoveAsync($"{_element.Key}");

        // 1. Start the request; it reads the old snapshot and parks before the cache write-back.
        Task<IPublishedElement?> staleRead = Task.Run(() => _elementCacheService.GetByKeyAsync(_element.Key, false));
        await readReachedDatabase.Task;

        // 2. A publish refreshes the in-memory cache with the new snapshot (and bumps the generation).
        await _elementCacheService.RefreshMemoryCacheAsync(_element.Key);

        // 3. Let the parked request resume and perform its (now stale) write-back.
        releaseRead.TrySetResult();
        await staleRead;

        // Verify the stale read (returns the old content)
        var staleResult = staleRead.Result;
        Assert.IsNotNull(staleResult);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(OldName, staleResult!.Name, "Name should be served from the stale snapshot.");
            Assert.AreEqual(
                OldTitle,
                staleResult.GetProperty("title")?.GetSourceValue(),
                "Property value should be served from the stale snapshot.");
        });

        // 4. A fresh lookup must observe the refreshed content, not the clobbered stale snapshot.
        IPublishedElement? result = await _elementCacheService.GetByKeyAsync(_element.Key, false);

        Assert.IsNotNull(result);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(NewName, result!.Name, "Name was served from the clobbered stale snapshot.");
            Assert.AreEqual(
                NewTitle,
                result.GetProperty("title")?.GetSourceValue(),
                "Property value was served from the clobbered stale snapshot.");
        });

        // Verify that the published node has been added to the hybrid cache using the assumed key.
        var cachedNode = await _hybridCache.GetOrCreateAsync(
            $"{_element.Key}",
            _ => new ValueTask<ContentCacheNode?>((ContentCacheNode?)null));

        Assert.IsNotNull(cachedNode);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(newNode.Key, cachedNode!.Key, "Cached node should be the refreshed node.");
            Assert.AreEqual(NewName, cachedNode!.Data?.Name, "Cached node should hold the refreshed name.");
        });
    }

    [Test]
    public async Task Batched_Read_Through_Does_Not_Clobber_A_Concurrent_Refresh()
    {
        ContentCacheNode oldNode = BuildPublishedNode(OldName, OldTitle);
        ContentCacheNode newNode = BuildPublishedNode(NewName, NewTitle);

        var readReachedDatabase = new TaskCompletionSource();
        var releaseRead = new TaskCompletionSource();
        _databaseCacheRepository
            .Setup(x => x.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false))
            .Returns(async () =>
            {
                readReachedDatabase.TrySetResult();
                await releaseRead.Task;
                return [oldNode];
            });

        _databaseCacheRepository
            .Setup(x => x.GetElementSourceForPublishStatesAsync(_element.Key))
            .ReturnsAsync((null, newNode));

        await _hybridCache.RemoveAsync($"{_element.Key}");

        Task<IReadOnlyList<IPublishedElement>> staleRead = Task.Run(() => _elementCacheService.GetByKeysAsync([_element.Key], false));
        await readReachedDatabase.Task;

        await _elementCacheService.RefreshMemoryCacheAsync(_element.Key);

        releaseRead.TrySetResult();
        IReadOnlyList<IPublishedElement> staleResult = await staleRead;

        Assert.AreEqual(OldName, staleResult.Single().Name, "The set is served from the stale snapshot.");

        // The stale write-back is skipped, so a fresh lookup observes the refreshed content.
        IReadOnlyList<IPublishedElement> result = await _elementCacheService.GetByKeysAsync([_element.Key], false);

        Assert.AreEqual(NewName, result.Single().Name, "Name was served from the clobbered stale snapshot.");
        Assert.AreEqual(NewTitle, result.Single().GetProperty("title")?.GetSourceValue());
    }

    [Test]
    public async Task Synchronous_Read_Through_Does_Not_Clobber_A_Concurrent_Refresh()
    {
        ContentCacheNode oldNode = BuildPublishedNode(OldName, OldTitle);
        ContentCacheNode newNode = BuildPublishedNode(NewName, NewTitle);

        // The synchronous read-through reads the pre-publish (old) snapshot, then blocks before writing back.
        using var readReachedDatabase = new ManualResetEventSlim();
        using var releaseRead = new ManualResetEventSlim();
        _databaseCacheRepository
            .Setup(x => x.GetElementSource(_element.Key, false))
            .Returns(() =>
            {
                readReachedDatabase.Set();
                releaseRead.Wait(TimeSpan.FromSeconds(10));
                return oldNode;
            });

        // The refresh after a commit reads the new snapshot.
        _databaseCacheRepository
            .Setup(x => x.GetElementSourceForPublishStatesAsync(_element.Key))
            .ReturnsAsync((null, newNode));

        await _hybridCache.RemoveAsync($"{_element.Key}");

        // 1. Start the synchronous request; it reads the old snapshot and blocks before the cache write-back.
        Task<IPublishedElement?> staleRead = Task.Run(() => _elementCacheService.GetByKey(_element.Key, false));
        Assert.That(readReachedDatabase.Wait(TimeSpan.FromSeconds(10)), Is.True, "The read never reached the database.");

        // 2. The refresh stores the new snapshot (and bumps the generation).
        await _elementCacheService.RefreshMemoryCacheAsync(_element.Key);

        // 3. Let the blocked request resume and perform its (now stale) write-back.
        releaseRead.Set();
        IPublishedElement? staleResult = await staleRead;

        Assert.IsNotNull(staleResult);
        Assert.AreEqual(OldName, staleResult!.Name, "Name should be served from the stale snapshot.");

        // 4. A fresh synchronous lookup must observe the refreshed content, not the stale snapshot.
        IPublishedElement? result = _elementCacheService.GetByKey(_element.Key, false);

        Assert.IsNotNull(result);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(NewName, result!.Name, "Name was served from the clobbered stale snapshot.");
            Assert.AreEqual(
                NewTitle,
                result.GetProperty("title")?.GetSourceValue(),
                "Property value was served from the clobbered stale snapshot.");
        });

        _databaseCacheRepository.Verify(
            x => x.GetElementSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never,
            "The synchronous read must not use the asynchronous repository read.");
    }
}
