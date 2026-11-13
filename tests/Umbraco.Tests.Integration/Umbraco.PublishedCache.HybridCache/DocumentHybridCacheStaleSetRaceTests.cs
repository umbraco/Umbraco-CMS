using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// Reproduces the read-through "stale-set clobber" race in <see cref="DocumentCacheService"/>:
/// a request reads a pre-publish snapshot from the backing store and then writes it back into the
/// in-memory cache *after* a concurrent publish has already refreshed it, leaving memory permanently
/// stale (or, for Block List, empty) until a full cache clear. The generation guard must reject the
/// stale write-back so the refreshed value survives.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheStaleSetRaceTests : UmbracoIntegrationTestWithContent
{
    private const string OldName = "Old Page";
    private const string OldTitle = "Old Title";
    private const string NewName = "New Page";
    private const string NewTitle = "New Title";

    private Mock<IDatabaseCacheRepository> _databaseCacheRepository;
    private DocumentCacheService _documentCacheService;
    private Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private ContentCacheNode BuildPublishedNode(string name, string title)
    {
        var data = new ContentData(
            name,
            null,
            1,
            Textpage.UpdateDate,
            Textpage.CreatorId,
            -1,
            true,
            new Dictionary<string, PropertyData[]>
            {
                ["title"] = [new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = title }],
            },
            null);

        return new ContentCacheNode
        {
            ContentTypeId = Textpage.ContentTypeId,
            CreatorId = Textpage.CreatorId,
            CreateDate = Textpage.CreateDate,
            Id = Textpage.Id,
            Key = Textpage.Key,
            SortOrder = 0,
            Data = data,
            IsDraft = false,
        };
    }

    [SetUp]
    public void SetUpService()
    {
        _databaseCacheRepository = new Mock<IDatabaseCacheRepository>();
        _hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();

        var publishStatus = new Mock<IDocumentPublishStatusQueryService>();
        publishStatus.Setup(x => x.IsPublishedInAnyCulture(It.IsAny<Guid>())).Returns(true);
        publishStatus.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>())).Returns(true);

        _documentCacheService = new DocumentCacheService(
            _databaseCacheRepository.Object,
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ICoreScopeProvider>(),
            _hybridCache,
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            Array.Empty<IDocumentSeedKeyProvider>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewSessionService>(),
            publishStatus.Object,
            new NullLogger<DocumentCacheService>(),
            new ConvertedPublishedContentCacheFactory(null, new NullLogger<ConvertedPublishedContentCacheFactory>()),
            Mock.Of<IPreviewAccessEvaluator>(evaluator => evaluator.CanPreviewDocument(It.IsAny<IPublishedContent>())),
            Mock.Of<IContentCacheDataSerializerFactory>());
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
            .Setup(x => x.GetDocumentSourceAsync(Textpage.Key, false))
            .Returns(async () =>
            {
                readReachedDatabase.TrySetResult();
                await releaseRead.Task;
                return oldNode;
            });

        // The publish-time memory refresh reads the new snapshot from the database cache.
        _databaseCacheRepository
            .Setup(x => x.GetDocumentSourceForPublishStatesAsync(Textpage.Key))
            .ReturnsAsync((null, newNode));

        // Ensure the published entry is absent so the request below is a genuine read-through.
        await _hybridCache.RemoveAsync($"{Textpage.Key}");

        // 1. Start the request; it reads the old snapshot and parks before the cache write-back.
        Task<IPublishedContent?> staleRead = Task.Run(() => _documentCacheService.GetByKeyAsync(Textpage.Key, false));
        await readReachedDatabase.Task;

        // 2. A publish refreshes the in-memory cache with the new snapshot (and bumps the generation).
        await _documentCacheService.RefreshMemoryCacheAsync(Textpage.Key);

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
        IPublishedContent? result = await _documentCacheService.GetByKeyAsync(Textpage.Key, false);

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
            $"{Textpage.Key}",
            _ => new ValueTask<ContentCacheNode?>((ContentCacheNode?)null));

        Assert.IsNotNull(cachedNode);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(newNode.Key, cachedNode!.Key, "Cached node should be the refreshed node.");
            Assert.AreEqual(NewName, cachedNode!.Data?.Name, "Cached node should hold the refreshed name.");
        });
    }

    [Test]
    public async Task Synchronous_Read_Through_Does_Not_Clobber_A_Concurrent_Synchronous_Refresh()
    {
        ContentCacheNode oldNode = BuildPublishedNode(OldName, OldTitle);
        ContentCacheNode newNode = BuildPublishedNode(NewName, NewTitle);

        // The synchronous read-through reads the pre-publish (old) snapshot, then blocks before writing back.
        using var readReachedDatabase = new ManualResetEventSlim();
        using var releaseRead = new ManualResetEventSlim();
        _databaseCacheRepository
            .Setup(x => x.GetDocumentSource(Textpage.Key, false))
            .Returns(() =>
            {
                readReachedDatabase.Set();
                releaseRead.Wait(TimeSpan.FromSeconds(10));
                return oldNode;
            });

        // The synchronous refresh after a commit reads the new snapshot.
        _databaseCacheRepository
            .Setup(x => x.GetDocumentSourceForPublishStates(Textpage.Key))
            .Returns((null, newNode));

        await _hybridCache.RemoveAsync($"{Textpage.Key}");

        // 1. Start the synchronous request; it reads the old snapshot and blocks before the cache write-back.
        Task<IPublishedContent?> staleRead = Task.Run(() => _documentCacheService.GetByKey(Textpage.Key, false));
        Assert.That(readReachedDatabase.Wait(TimeSpan.FromSeconds(10)), Is.True, "The read never reached the database.");

        // 2. The synchronous refresh stores the new snapshot (and bumps the generation).
        _documentCacheService.RefreshMemoryCache(Textpage.Key);

        // 3. Let the blocked request resume and perform its (now stale) write-back.
        releaseRead.Set();
        IPublishedContent? staleResult = await staleRead;

        Assert.IsNotNull(staleResult);
        Assert.AreEqual(OldName, staleResult!.Name, "Name should be served from the stale snapshot.");

        // 4. A fresh synchronous lookup must observe the refreshed content, not the stale snapshot.
        IPublishedContent? result = _documentCacheService.GetByKey(Textpage.Key, false);

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
            x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never,
            "The synchronous read must not use the asynchronous repository read.");
    }
}
