using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementHybridCacheMockTests : UmbracoIntegrationTest
{
    private IPublishedElementCache _mockedCache;
    private Mock<IDatabaseCacheRepository> _mockDatabaseCacheRepository;
    private IElementCacheService _elementCacheService;
    private Element _element;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    [SetUp]
    public async Task SetUp()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType();
        await GetRequiredService<IContentTypeService>().CreateAsync(elementType, Constants.Security.SuperUserKey);

        _element = ElementBuilder.CreateSimpleElement(elementType);
        GetRequiredService<IElementService>().Save(_element);

        ContentCacheNode draftTestCacheNode = BuildCacheNode(isDraft: true);
        ContentCacheNode publishedTestCacheNode = BuildCacheNode(isDraft: false);

        _mockDatabaseCacheRepository = new Mock<IDatabaseCacheRepository>();

        _mockDatabaseCacheRepository.Setup(r => r.GetElementSourceAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync(draftTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetElementSourceAsync(It.IsAny<Guid>(), false))
            .ReturnsAsync(publishedTestCacheNode);

        _mockDatabaseCacheRepository.Setup(r => r.GetElementSource(It.IsAny<Guid>(), true))
            .Returns(draftTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetElementSource(It.IsAny<Guid>(), false))
            .Returns(publishedTestCacheNode);

        _mockDatabaseCacheRepository.Setup(r => r.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), true))
            .ReturnsAsync((IEnumerable<Guid> keys, bool _) => keys.Contains(_element.Key) ? [draftTestCacheNode] : []);
        _mockDatabaseCacheRepository.Setup(r => r.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false))
            .ReturnsAsync((IEnumerable<Guid> keys, bool _) => keys.Contains(_element.Key) ? [publishedTestCacheNode] : []);
        _mockDatabaseCacheRepository.Setup(r => r.GetElementSources(It.IsAny<IEnumerable<Guid>>(), true))
            .Returns((IEnumerable<Guid> keys, bool _) => keys.Contains(_element.Key) ? [draftTestCacheNode] : []);
        _mockDatabaseCacheRepository.Setup(r => r.GetElementSources(It.IsAny<IEnumerable<Guid>>(), false))
            .Returns((IEnumerable<Guid> keys, bool _) => keys.Contains(_element.Key) ? [publishedTestCacheNode] : []);

        _elementCacheService = CreateCacheService(canPreview: true);
        _mockedCache = new ElementCache(_elementCacheService);
    }

    [Test]
    public async Task Synchronous_Lookup_By_Key_Reads_The_Database_Synchronously()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_element.Key}+draft");

        IPublishedElement? element = _mockedCache.GetById(true, _element.Key);

        Assert.AreEqual(_element.Key, element?.Key);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSource(_element.Key, true), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    // The synchronous member blocks on this task, so with no distributed cache to wait for it must have completed by
    // the time it is returned. A task that has not means some step of the shared read logic awaited asynchronous
    // work, and synchronous callers are blocking on it again.
    [TestCase(true)]
    [TestCase(false)]
    public async Task Synchronous_Read_Logic_Completes_Synchronously_On_A_Cache_Miss(bool preview)
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_element.Key}+draft");
        await hybridCache.RemoveAsync($"{_element.Key}");

        Task<IPublishedElement?> task = ((ElementCacheService)_elementCacheService).GetByKeyWithSynchronousReadsAsync(_element.Key, preview);

        Assert.IsTrue(task.IsCompleted);
        Assert.AreEqual(_element.Key, (await task)?.Key);
    }

    [Test]
    public async Task Synchronous_Read_Logic_Completes_Synchronously_When_Rereading_A_Refused_Draft()
    {
        ElementCacheService refusingCacheService = CreateCacheService(canPreview: false);

        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{_element.Key}+draft");
        await hybridCache.RemoveAsync($"{_element.Key}");

        Task<IPublishedElement?> task = refusingCacheService.GetByKeyWithSynchronousReadsAsync(_element.Key, true);

        Assert.IsTrue(task.IsCompleted);
        Assert.AreEqual(_element.Key, (await task)?.Key);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSource(_element.Key, false), Times.Once);
    }

    [Test]
    public async Task Synchronous_Batched_Lookup_Reads_The_Database_Synchronously()
    {
        await RemoveFromHybridCache(_element.Key);

        IReadOnlyList<IPublishedElement> result = _mockedCache.GetByIds(false, [_element.Key, Guid.NewGuid()]);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_element.Key, result[0].Key);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSources(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Never);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSource(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task Synchronous_Batched_Preview_Lookup_Rereads_Refused_Drafts_Synchronously()
    {
        ElementCacheService refusingCacheService = CreateCacheService(canPreview: false);
        await RemoveFromHybridCache(_element.Key);

        IReadOnlyList<IPublishedElement> result = refusingCacheService.GetByKeys([_element.Key], true);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_element.Key, result[0].Key);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSources(It.IsAny<IEnumerable<Guid>>(), true), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSources(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Never);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Synchronous_Batched_Read_Logic_Completes_Synchronously_On_A_Cache_Miss(bool preview)
    {
        await RemoveFromHybridCache(_element.Key);

        Task<IReadOnlyList<IPublishedElement>> task = ((ElementCacheService)_elementCacheService).GetByKeysWithSynchronousReadsAsync([_element.Key], preview);

        Assert.IsTrue(task.IsCompleted);
        Assert.AreEqual(_element.Key, (await task).Single().Key);
    }

    [Test]
    public async Task Synchronous_Batched_Read_Logic_Completes_Synchronously_When_Rereading_Refused_Drafts()
    {
        ElementCacheService refusingCacheService = CreateCacheService(canPreview: false);
        await RemoveFromHybridCache(_element.Key);

        Task<IReadOnlyList<IPublishedElement>> task = refusingCacheService.GetByKeysWithSynchronousReadsAsync([_element.Key], true);

        Assert.IsTrue(task.IsCompleted);
        Assert.AreEqual(_element.Key, (await task).Single().Key);
    }

    [Test]
    public async Task GetByKeysAsync_Batches_The_Database_Read_And_Never_Calls_Single_Per_Item()
    {
        await RemoveFromHybridCache(_element.Key);

        IReadOnlyList<IPublishedElement> result = await _elementCacheService.GetByKeysAsync([_element.Key, Guid.NewGuid(), Guid.NewGuid()], false);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(_element.Key, result[0].Key);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_Reads_Only_The_Keys_Missing_From_The_Caches()
    {
        await RemoveFromHybridCache(_element.Key);
        Guid coldKey = Guid.NewGuid();

        // Warms the element into the caches, so only the other key is left to read.
        Assert.AreEqual(1, (await _elementCacheService.GetByKeysAsync([_element.Key], false)).Count);
        _mockDatabaseCacheRepository.Invocations.Clear();

        IReadOnlyList<IPublishedElement> result = await _elementCacheService.GetByKeysAsync([coldKey, _element.Key], false);

        Assert.AreEqual(_element.Key, result.Single().Key);
        _mockDatabaseCacheRepository.Verify(
            x => x.GetElementSourcesAsync(It.Is<IEnumerable<Guid>>(keys => keys.SequenceEqual(new[] { coldKey })), false),
            Times.Once);
    }

    [Test]
    public async Task GetByKeysAsync_Returns_Elements_In_Key_Order_And_Omits_Missing_Keys()
    {
        await RemoveFromHybridCache(_element.Key);
        Guid secondKey = Guid.NewGuid();
        Guid missingKey = Guid.NewGuid();
        ContentCacheNode firstNode = BuildCacheNode(isDraft: false);
        ContentCacheNode secondNode = BuildCacheNode(isDraft: false, secondKey);
        _mockDatabaseCacheRepository
            .Setup(x => x.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false))
            .ReturnsAsync([firstNode, secondNode]);

        IReadOnlyList<IPublishedElement> result = await _elementCacheService.GetByKeysAsync([secondKey, missingKey, _element.Key, secondKey], false);

        // A repeated key resolves at every occurrence, as it would from repeated single lookups.
        CollectionAssert.AreEqual(new[] { secondKey, _element.Key, secondKey }, result.Select(x => x.Key));
    }

    [Test]
    public async Task GetByKeysAsync_Populates_The_Memory_Cache_So_Subsequent_Reads_Do_Not_Hit_The_Database()
    {
        await RemoveFromHybridCache(_element.Key);

        _ = await _elementCacheService.GetByKeysAsync([_element.Key], false);

        Assert.IsTrue(_elementCacheService.TryGetCached(_element.Key, false, out IPublishedElement? cached));
        Assert.IsNotNull(cached);
        Assert.IsNotNull(await _mockedCache.GetByIdAsync(_element.Key, false));
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_Honours_A_Cached_Null_Without_Querying_The_Database()
    {
        Guid missingKey = Guid.NewGuid();
        _mockDatabaseCacheRepository.Setup(x => x.GetElementSourceAsync(missingKey, false)).ReturnsAsync((ContentCacheNode?)null);

        Assert.IsNull(await _elementCacheService.GetByKeyAsync(missingKey, false));

        Assert.IsEmpty(await _elementCacheService.GetByKeysAsync([missingKey], false));
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeys_Caches_Null_For_Keys_The_Database_Does_Not_Return()
    {
        Guid unpublishedKey = Guid.NewGuid();

        Assert.IsEmpty(_elementCacheService.GetByKeys([unpublishedKey], false));
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSources(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);

        // The batched read caches the null as the single read-through does, so neither goes back to the database.
        Assert.IsEmpty(_elementCacheService.GetByKeys([unpublishedKey], false));
        Assert.IsNull(await _elementCacheService.GetByKeyAsync(unpublishedKey, false));
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSources(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetElementSourceAsync(unpublishedKey, It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_Does_Not_Read_The_Database_For_An_Empty_Key_Set()
    {
        Assert.IsEmpty(await _elementCacheService.GetByKeysAsync([], false));
        _mockDatabaseCacheRepository.VerifyNoOtherCalls();
    }

    private async Task RemoveFromHybridCache(Guid key)
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{key}+draft");
        await hybridCache.RemoveAsync($"{key}");
    }

    private ElementCacheService CreateCacheService(bool canPreview)
        => new(
            _mockDatabaseCacheRepository.Object,
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(),
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            Array.Empty<IElementSeedKeyProvider>(),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewSessionService>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            new NullLogger<ElementCacheService>(),
            new ConvertedPublishedContentCacheFactory(null, new NullLogger<ConvertedPublishedContentCacheFactory>()),
            Mock.Of<IPreviewAccessEvaluator>(evaluator => evaluator.CanPreviewElement(It.IsAny<IPublishedElement>()) == canPreview),
            Mock.Of<IContentCacheDataSerializerFactory>());

    private ContentCacheNode BuildCacheNode(bool isDraft, Guid? key = null)
        => new()
        {
            ContentTypeId = _element.ContentTypeId,
            CreatorId = _element.CreatorId,
            CreateDate = _element.CreateDate,
            Id = _element.Id,
            Key = key ?? _element.Key,
            SortOrder = 0,
            Data = new ContentData(
                _element.Name,
                null,
                1,
                _element.UpdateDate,
                _element.CreatorId,
                -1,
                isDraft is false,
                new Dictionary<string, PropertyData[]>(),
                null),
            IsDraft = isDraft,
        };
}
