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
public class DocumentHybridCacheMockTests : UmbracoIntegrationTestWithContent
{
    private IPublishedContentCache _mockedCache;
    private Mock<IDatabaseCacheRepository> _mockedNucacheRepository;
    private IDocumentCacheService _mockDocumentCacheService;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private CacheSettings _cacheSettings;

    [SetUp]
    public void SetUp()
    {
        _mockedNucacheRepository = new Mock<IDatabaseCacheRepository>();

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

        _mockedNucacheRepository.Setup(r => r.GetContentSourceAsync(It.IsAny<Guid>(), true))
            .ReturnsAsync(draftTestCacheNode);

        _mockedNucacheRepository.Setup(r => r.GetContentSourceAsync(It.IsAny<Guid>(), false))
            .ReturnsAsync(publishedTestCacheNode);

        _mockedNucacheRepository.Setup(r => r.GetContentByContentTypeKey(It.IsAny<IReadOnlyCollection<Guid>>(), ContentCacheDataSerializerEntityType.Document)).Returns(
            new List<ContentCacheNode>()
            {
                draftTestCacheNode,
            });

        _mockedNucacheRepository.Setup(r => r.DeleteContentItemAsync(It.IsAny<int>()));

        _mockDocumentCacheService = new DocumentCacheService(
            _mockedNucacheRepository.Object,
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<Microsoft.Extensions.Caching.Hybrid.HybridCache>(),
            GetRequiredService<IPublishedContentFactory>(),
            GetRequiredService<ICacheNodeFactory>(),
            GetSeedProviders(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IPreviewService>(),
            GetRequiredService<IPublishStatusQueryService>(),
            GetRequiredService<IDocumentNavigationQueryService>());

        _mockedCache = new DocumentCache(_mockDocumentCacheService,
            GetRequiredService<IPublishedContentTypeCache>(),
            GetRequiredService<IDocumentNavigationQueryService>(),
            GetRequiredService<IDocumentUrlService>(),
            new Lazy<IPublishedUrlProvider>(GetRequiredService<IPublishedUrlProvider>));
    }

    // We want to be able to alter the settings for the providers AFTER the test has started
    // So we'll manually create them with a magic options mock.
    private IEnumerable<IDocumentSeedKeyProvider> GetSeedProviders()
    {
        _cacheSettings = new CacheSettings();
        _cacheSettings.DocumentBreadthFirstSeedCount = 0;

        var mock = new Mock<IOptions<CacheSettings>>();
        mock.Setup(m => m.Value).Returns(() => _cacheSettings);

        return new List<IDocumentSeedKeyProvider>
        {
            new ContentTypeSeedKeyProvider(GetRequiredService<ICoreScopeProvider>(), GetRequiredService<IDatabaseCacheRepository>(), mock.Object),
            new DocumentBreadthFirstKeyProvider(GetRequiredService<IDocumentNavigationQueryService>(), mock.Object),
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
        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
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
        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Seeded_By_Id()
    {
        var schedule = new CultureAndScheduleModel
        {
            CulturesToPublishImmediately = new HashSet<string> { "*" }, Schedules = new ContentScheduleCollection(),
        };

        var publishResult = await ContentPublishingService.PublishAsync(Textpage.Key, schedule, Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);
        Textpage.Published = true;
        await _mockDocumentCacheService.DeleteItemAsync(Textpage);

        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _mockDocumentCacheService.SeedAsync(CancellationToken.None);
        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));

        var textPage = await _mockedCache.GetByIdAsync(Textpage.Id);
        AssertTextPage(textPage);

        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Seeded_By_Key()
    {
        var schedule = new CultureAndScheduleModel
        {
            CulturesToPublishImmediately = new HashSet<string> { "*" }, Schedules = new ContentScheduleCollection(),
        };

        var publishResult = await ContentPublishingService.PublishAsync(Textpage.Key, schedule, Constants.Security.SuperUserKey);
        Assert.IsTrue(publishResult.Success);
        Textpage.Published = true;
        await _mockDocumentCacheService.DeleteItemAsync(Textpage);

        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _mockDocumentCacheService.SeedAsync(CancellationToken.None);
        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Key);
        AssertTextPage(textPage);

        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Not_Seeded_If_Unpblished_By_Id()
    {

        await _mockDocumentCacheService.DeleteItemAsync(Textpage);

        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _mockDocumentCacheService.SeedAsync(CancellationToken.None);
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Id, true);
        AssertTextPage(textPage);

        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Not_Seeded_If_Unpublished_By_Key()
    {
        _cacheSettings.ContentTypeKeys = [ Textpage.ContentType.Key ];
        await _mockDocumentCacheService.DeleteItemAsync(Textpage);

        await _mockDocumentCacheService.SeedAsync(CancellationToken.None);
        var textPage = await _mockedCache.GetByIdAsync(Textpage.Key, true);
        AssertTextPage(textPage);

        _mockedNucacheRepository.Verify(x => x.GetContentSourceAsync(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
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
