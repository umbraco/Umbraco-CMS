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

        _mockDatabaseCacheRepository.Setup(r => r.GetContentSourceAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync(draftTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetContentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), true))
            .ReturnsAsync([draftTestCacheNode]);

        _mockDatabaseCacheRepository.Setup(r => r.GetContentSourceAsync(It.IsAny<Guid>(), false))
            .ReturnsAsync(publishedTestCacheNode);
        _mockDatabaseCacheRepository.Setup(r => r.GetContentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), false))
            .ReturnsAsync([publishedTestCacheNode]);

        _mockDatabaseCacheRepository.Setup(r => r.GetContentSourceForPublishStatesAsync(It.IsAny<Guid>()))
            .ReturnsAsync((draftTestCacheNode, publishedTestCacheNode));

        _mockDatabaseCacheRepository.Setup(r => r.GetContentByContentTypeKey(It.IsAny<IReadOnlyCollection<Guid>>(), ContentCacheDataSerializerEntityType.Document)).Returns(
            new List<ContentCacheNode>()
            {
                draftTestCacheNode,
            });

        _mockDatabaseCacheRepository.Setup(r => r.DeleteContentItemAsync(It.IsAny<int>()));

        var mockedPublishedStatusService = new Mock<IPublishStatusQueryService>();
        mockedPublishedStatusService.Setup(x => x.IsDocumentPublishedInAnyCulture(It.IsAny<Guid>())).Returns(true);
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
            new NullLogger<DocumentCacheService>());

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
        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
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
        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
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
        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));

        var textPage = await _mockedCache.GetByIdAsync(Textpage.Id);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));
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
        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Key);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourcesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Not_Seeded_If_Unpblished_By_Id()
    {

        await _documentCacheService.DeleteItemAsync(Textpage);

        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _documentCacheService.SeedAsync(CancellationToken.None);
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Id, true);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Not_Seeded_If_Unpublished_By_Key()
    {
        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _documentCacheService.DeleteItemAsync(Textpage);

        await _documentCacheService.SeedAsync(CancellationToken.None);
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Key, true);
        AssertTextPage(textPage);

        _mockDatabaseCacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
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
            x => x.GetContentSourceForPublishStatesAsync(Textpage.Key),
            Times.Exactly(1));

        // Verify individual GetContentSourceAsync was NOT called
        _mockDatabaseCacheRepository.Verify(
            x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
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
            x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
        _mockDatabaseCacheRepository.Verify(
            x => x.GetContentSourceForPublishStatesAsync(It.IsAny<Guid>()),
            Times.Exactly(1));
    }

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
