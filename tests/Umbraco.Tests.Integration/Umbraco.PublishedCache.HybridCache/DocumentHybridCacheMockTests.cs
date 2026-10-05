using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentHybridCacheMockTests : UmbracoIntegrationTestWithContent
{
    private IPublishedContentCache _mockedCache;
    private Mock<IDatabaseCacheRepository> _mockDatabaseCacheRepository;
    private IDocumentCacheService _documentCacheService;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private CacheSettings _cacheSettings;

    [SetUp]
    public void SetUp()
    {
        _mockDatabaseCacheRepository = new Mock<IDatabaseCacheRepository>();

        var contentData = new ContentData(
            Textpage.Name,
            null,
            1,
            Textpage.UpdateDate,
            Textpage.CreatorId,
            -1,
            false,
            new Dictionary<string, PropertyData[]>(),
            null);


        var draftTestCacheNode = new ContentCacheNode()
        {
            ContentTypeId = Textpage.ContentTypeId,
            CreatorId = Textpage.CreatorId,
            CreateDate = Textpage.CreateDate,
            Id = Textpage.Id,
            Key = Textpage.Key,
            SortOrder = 0,
            Data = contentData,
            IsDraft = true,
        };

        var publishedTestCacheNode = new ContentCacheNode()
        {
            ContentTypeId = Textpage.ContentTypeId,
            CreatorId = Textpage.CreatorId,
            CreateDate = Textpage.CreateDate,
            Id = Textpage.Id,
            Key = Textpage.Key,
            SortOrder = 0,
            Data = contentData,
            IsDraft = false,
        };

        _mockDatabaseCacheRepository.Setup(r => r.GetDocumentSourceAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync(draftTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), true))
            .ReturnsAsync([draftTestCacheNode]);

        _mockDatabaseCacheRepository.Setup(r => r.GetDocumentSourceAsync(It.IsAny<Guid>(), false))
            .ReturnsAsync(publishedTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false))
            .ReturnsAsync([publishedTestCacheNode]);

        _mockDatabaseCacheRepository.Setup(r => r.GetDocumentSourceForPublishStatesAsync(It.IsAny<Guid>()))
            .ReturnsAsync((draftTestCacheNode, publishedTestCacheNode));

        _mockDatabaseCacheRepository.Setup(r => r.GetContentByContentTypeKey(It.IsAny<IReadOnlyCollection<Guid>>(), ContentCacheDataSerializerEntityType.Document)).Returns(
            new List<ContentCacheNode>()
            {
                draftTestCacheNode,
            });

        _mockDatabaseCacheRepository.Setup(r => r.DeleteContentItemAsync(It.IsAny<int>()));

        var mockedPublishedStatusService = new Mock<IPublishStatusQueryService>();
        mockedPublishedStatusService.Setup(x => x.IsPublishedInAnyCulture(It.IsAny<Guid>())).Returns(true);
        mockedPublishedStatusService.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>())).Returns(true);

        _documentCacheService = new DocumentCacheService(
            _mockDatabaseCacheRepository.Object,
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(),
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            GetSeedProviders(mockedPublishedStatusService.Object),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewService>(),
            mockedPublishedStatusService.Object,
            new NullLogger<DocumentCacheService>(),
            new ConvertedPublishedContentCacheFactory(null, new NullLogger<ConvertedPublishedContentCacheFactory>()));

        _mockedCache = new DocumentCache(
            _documentCacheService,
            GetRequiredService<IPublishedContentTypeCache>(),
            GetRequiredService<IDocumentNavigationQueryService>(),
            GetRequiredService<IDocumentUrlService>(),
            new Lazy<IPublishedUrlProvider>(GetRequiredService<IPublishedUrlProvider>));
    }

    // We want to be able to alter the settings for the providers AFTER the test has started
    // So we'll manually create them with a magic options mock.
    private IEnumerable<IDocumentSeedKeyProvider> GetSeedProviders(IPublishStatusQueryService publishStatusQueryService)
    {
        _cacheSettings = new CacheSettings
        {
            DocumentBreadthFirstSeedCount = 0
        };

        var mock = new Mock<IOptions<CacheSettings>>();
        mock.Setup(m => m.Value).Returns(() => _cacheSettings);

        return new List<IDocumentSeedKeyProvider>
        {
            new ContentTypeSeedKeyProvider(GetRequiredService<ICoreScopeProvider>(), GetRequiredService<IDatabaseCacheRepository>(), mock.Object, publishStatusQueryService),
            new DocumentBreadthFirstKeyProvider(GetRequiredService<IDocumentNavigationQueryService>(), mock.Object, publishStatusQueryService),
        };
    }

    [Test]
    public async Task Content_Is_Cached_By_Key()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{Textpage.Key}+draft");
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Key, true);
        var textPage2 = await _mockedCache.GetByIdAsync(Textpage.Key, true);
        AssertTextPage(textPage);
        AssertTextPage(textPage2);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Cached_By_Id()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{Textpage.Key}+draft");
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Id, true);
        var textPage2 = await _mockedCache.GetByIdAsync(Textpage.Id, true);
        AssertTextPage(textPage);
        AssertTextPage(textPage2);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Seeded_By_Id()
    {
        var schedule = new CulturePublishScheduleModel
        {
            Culture = Constants.System.InvariantCulture,
        };

        var publishResult = await ContentPublishingService.PublishAsync(Textpage.Key, [schedule], Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);
        Textpage.Published = true;
        await _documentCacheService.DeleteItemAsync(Textpage);

        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _documentCacheService.SeedAsync(CancellationToken.None);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));

        var textPage = await _mockedCache.GetByIdAsync(Textpage.Id);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Seeded_By_Key()
    {
        var schedule = new CulturePublishScheduleModel
        {
            Culture = Constants.System.InvariantCulture,
        };

        var publishResult = await ContentPublishingService.PublishAsync(Textpage.Key, [schedule], Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);
        Textpage.Published = true;
        await _documentCacheService.DeleteItemAsync(Textpage);

        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _documentCacheService.SeedAsync(CancellationToken.None);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Key);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Not_Seeded_If_Unpblished_By_Id()
    {

        await _documentCacheService.DeleteItemAsync(Textpage);

        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _documentCacheService.SeedAsync(CancellationToken.None);
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Id, true);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Not_Seeded_If_Unpublished_By_Key()
    {
        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _documentCacheService.DeleteItemAsync(Textpage);

        await _documentCacheService.SeedAsync(CancellationToken.None);
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Key, true);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task GetByKeysAsync_BatchesDatabaseRead_AndNeverCallsSinglePerItem()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{Textpage.Key}");

        // A set of cold keys (only Textpage resolves against the mocked batch).
        var keys = new[] { Textpage.Key, Guid.NewGuid(), Guid.NewGuid() };

        IReadOnlyList<IPublishedContent> result = await _documentCacheService.GetByKeysAsync(keys, false);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(Textpage.Key, result[0].Key);

        // The single batched query is used; the per-item single query is never called.
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_PopulatesMemoryCache_SoSubsequentReadsDoNotHitDatabase()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{Textpage.Key}");

        _ = await _documentCacheService.GetByKeysAsync([Textpage.Key], false);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);

        // Now served from the in-memory (L0) cache — the sync fast path hits.
        Assert.IsTrue(_documentCacheService.TryGetCached(Textpage.Key, false, out IPublishedContent? cached));
        Assert.IsNotNull(cached);

        // And a further retrieval makes no additional database call.
        var again = await _mockedCache.GetByIdAsync(Textpage.Key, false);
        Assert.IsNotNull(again);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false), Times.Once);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_HonoursCachedNull_WithoutQueryingDatabase()
    {
        Guid missingKey = Guid.NewGuid();

        // Nothing in the database for this key, so the per-key read-through caches a null against it.
        _mockDatabaseCacheRepository.Setup(x => x.GetDocumentSourceAsync(missingKey, false)).ReturnsAsync((ContentCacheNode?)null);
        _mockDatabaseCacheRepository
            .Setup(x => x.GetDocumentSourcesAsync(It.Is<IEnumerable<Guid>>(keys => keys.Contains(missingKey)), false))
            .ReturnsAsync(Array.Empty<ContentCacheNode>());

        Assert.IsNull(await _documentCacheService.GetByKeyAsync(missingKey, false));
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(missingKey, false), Times.Once);

        IReadOnlyList<IPublishedContent> result = await _documentCacheService.GetByKeysAsync([missingKey], false);

        // A cached null means "already known to resolve to nothing", so the batched path has to serve it
        // from the cache as the per-key path does. Re-reading such a key on every request is the
        // regression reported in #18869.
        Assert.IsEmpty(result);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_EmptyInput_ReturnsEmpty()
    {
        IReadOnlyList<IPublishedContent> result = await _documentCacheService.GetByKeysAsync(Array.Empty<Guid>(), false);

        Assert.IsEmpty(result);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Never);
        _mockDatabaseCacheRepository.Verify(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetByKeysAsync_DuplicateKeys_ResolveToSameItemAtEveryOccurrence()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{Textpage.Key}");

        // A batched lookup preserves input multiplicity rather than collapsing duplicates, while only
        // looking the key up once against the database.
        var keys = new[] { Textpage.Key, Textpage.Key };

        IReadOnlyList<IPublishedContent> result = await _documentCacheService.GetByKeysAsync(keys, false);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(Textpage.Key, result[0].Key);
        Assert.AreEqual(Textpage.Key, result[1].Key);

        _mockDatabaseCacheRepository.Verify(
            x => x.GetDocumentSourcesAsync(It.Is<IEnumerable<Guid>>(k => k.Count() == 1 && k.Contains(Textpage.Key)), false),
            Times.Once);
    }

    [Test]
    public async Task GetByKeysAsync_PreservesInputOrder_AcrossMixedCacheHits()
    {
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{Textpage.Key}");
        await hybridCache.RemoveAsync($"{Subpage.Key}");
        await hybridCache.RemoveAsync($"{Subpage2.Key}");

        var nodesByKey = new Dictionary<Guid, ContentCacheNode>
        {
            [Textpage.Key] = BuildCacheNode(Textpage),
            [Subpage.Key] = BuildCacheNode(Subpage),
            [Subpage2.Key] = BuildCacheNode(Subpage2),
        };

        _mockDatabaseCacheRepository
            .Setup(x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), false))
            .ReturnsAsync((Guid key, bool _) => nodesByKey.GetValueOrDefault(key));
        _mockDatabaseCacheRepository
            .Setup(x => x.GetDocumentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false))
            .ReturnsAsync((IEnumerable<Guid> keys, bool _) => keys.Where(nodesByKey.ContainsKey).Select(k => nodesByKey[k]));

        // Warm Subpage into L0 ahead of time; Textpage and Subpage2 stay cold, so the batched call below
        // resolves a mix of tiers in one go.
        _ = await _documentCacheService.GetByKeyAsync(Subpage.Key, false);

        Guid[] requestOrder = [Subpage2.Key, Subpage.Key, Textpage.Key];
        IReadOnlyList<IPublishedContent> result = await _documentCacheService.GetByKeysAsync(requestOrder, false);

        CollectionAssert.AreEqual(requestOrder, result.Select(x => x.Key));
    }

    [Test]
    public async Task RefreshMemoryCache_Fetches_Draft_And_Published()
    {
        // Arrange
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();

        // Clear both draft and published cache entries.
        await hybridCache.RemoveAsync($"{Textpage.Key}+draft");
        await hybridCache.RemoveAsync($"{Textpage.Key}");

        // Act
        await _documentCacheService.RefreshMemoryCacheAsync(Textpage.Key);

        // Assert - verify only a single call was made to the combined method for retrieving both states.
        _mockDatabaseCacheRepository.Verify(
            x => x.GetDocumentSourceForPublishStatesAsync(Textpage.Key),
            Times.Exactly(1));

        // Verify individual GetDocumentSourceAsync was NOT called
        _mockDatabaseCacheRepository.Verify(
            x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);

        // Verify content is now cached - fetching should not hit the repository again.
        var draftPage = await _mockedCache.GetByIdAsync(Textpage.Key, true);
        var publishedPage = await _mockedCache.GetByIdAsync(Textpage.Key, false);

        Assert.IsNotNull(draftPage);
        Assert.IsNotNull(publishedPage);
        Assert.AreEqual(Textpage.Name, draftPage.Name);
        Assert.AreEqual(Textpage.Name, publishedPage.Name);

        // Verify no additional repository calls were made (content served from cache).
        _mockDatabaseCacheRepository.Verify(
            x => x.GetDocumentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
        _mockDatabaseCacheRepository.Verify(
            x => x.GetDocumentSourceForPublishStatesAsync(It.IsAny<Guid>()),
            Times.Exactly(1));
    }

    [Test]
    public async Task Null_Is_Not_Cached_When_Content_Exists_But_Ancestor_Check_Fails()
    {
        // Arrange - create a new DocumentCacheService with a controllable HasPublishedAncestorPath mock.
        var ancestorCheckReturnsTrue = false;

        var controllableMock = new Mock<IPublishStatusQueryService>();
        controllableMock.Setup(x => x.IsDocumentPublishedInAnyCulture(It.IsAny<Guid>())).Returns(true);
        controllableMock.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>()))
            .Returns(() => ancestorCheckReturnsTrue);

        var controlledCacheService = new DocumentCacheService(
            _mockDatabaseCacheRepository.Object,
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(),
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            GetSeedProviders(controllableMock.Object),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewService>(),
            controllableMock.Object,
            new NullLogger<DocumentCacheService>(),
            new ConvertedPublishedContentCacheFactory(null, new NullLogger<ConvertedPublishedContentCacheFactory>()));

        var controlledCache = new DocumentCache(
            controlledCacheService,
            GetRequiredService<IPublishedContentTypeCache>(),
            GetRequiredService<IDocumentNavigationQueryService>(),
            GetRequiredService<IDocumentUrlService>(),
            new Lazy<IPublishedUrlProvider>(GetRequiredService<IPublishedUrlProvider>));

        // Clear any existing cache entry for this key.
        var hybridCache = GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        await hybridCache.RemoveAsync($"{Textpage.Key}");

        // Act 1 - ancestor check returns false, so GetByKeyAsync should return null.
        var firstResult = await controlledCache.GetByIdAsync(Textpage.Key, false);
        Assert.IsNull(firstResult, "First call should return null when ancestor check fails");

        // Act 2 - now the ancestor check returns true.
        ancestorCheckReturnsTrue = true;
        var secondResult = await controlledCache.GetByIdAsync(Textpage.Key, false);

        // Assert - the null from step 1 should NOT have been cached, so step 2 should
        // hit the database again and return the content.
        Assert.IsNotNull(secondResult, "Second call should return content because null should not have been cached when ancestor check failed");
    }

    private static ContentCacheNode BuildCacheNode(Content content) =>
        new()
        {
            ContentTypeId = content.ContentTypeId,
            CreatorId = content.CreatorId,
            CreateDate = content.CreateDate,
            Id = content.Id,
            Key = content.Key,
            SortOrder = content.SortOrder,
            Data = new ContentData(
                content.Name,
                null,
                1,
                content.UpdateDate,
                content.CreatorId,
                -1,
                false,
                new Dictionary<string, PropertyData[]>(),
                null),
            IsDraft = false,
        };

    private void AssertTextPage(IPublishedContent textPage)
    {
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(textPage);
            Assert.AreEqual(Textpage.Name, textPage.Name);
            Assert.AreEqual(Textpage.Published, textPage.IsPublished());
        });
        AssertProperties(Textpage.Properties, textPage.Properties);
    }

    private void AssertProperties(IPropertyCollection propertyCollection, IEnumerable<IPublishedProperty> publishedProperties)
    {
        foreach (var prop in propertyCollection)
        {
            AssertProperty(prop, publishedProperties.First(x => x.Alias == prop.Alias));
        }
    }

    private void AssertProperty(IProperty property, IPublishedProperty publishedProperty)
    {
        Assert.Multiple(() =>
        {
            Assert.AreEqual(property.Alias, publishedProperty.Alias);
            Assert.AreEqual(property.PropertyType.Alias, publishedProperty.PropertyType.Alias);
        });
    }
}
