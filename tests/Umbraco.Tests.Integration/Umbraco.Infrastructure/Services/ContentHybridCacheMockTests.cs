using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ContentHybridCacheMockTests : UmbracoIntegrationTestWithContent
{
    private IPublishedContentHybridCache _mockedCache;
    private Mock<INuCacheContentRepository> _mockedNucacheRepository;
    private IContentCacheService _mockContentCacheService;

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.AddUmbracoHybridCache();

    [SetUp]
    public void SetUp()
    {
        _mockedNucacheRepository = new Mock<INuCacheContentRepository>();

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
        _mockedNucacheRepository.Setup(r => r.GetContentSource(It.IsAny<int>())).Returns(
            new ContentCacheNode()
            {
                ContentTypeId = Textpage.ContentTypeId,
                CreatorId = Textpage.CreatorId,
                CreateDate = Textpage.CreateDate,
                Id = Textpage.Id,
                Key = Textpage.Key,
                SortOrder = 0,
                Path = Textpage.Path,
                Draft = contentData,
                Published = null,
            });

        _mockedNucacheRepository.Setup(r => r.GetTypeContentSources(It.IsAny<IReadOnlyCollection<int>?>())).Returns(
            new List<ContentCacheNode>()
            {
                new()
                {
                    ContentTypeId = Textpage.ContentTypeId,
                    CreatorId = Textpage.CreatorId,
                    CreateDate = Textpage.CreateDate,
                    Id = Textpage.Id,
                    Key = Textpage.Key,
                    SortOrder = 0,
                    Path = Textpage.Path,
                    Draft = contentData,
                    Published = null,
                },
            });

        _mockedNucacheRepository.Setup(r => r.DeleteContentItem(It.IsAny<int>()));

        _mockContentCacheService = new ContentCacheService(
            _mockedNucacheRepository.Object,
            GetRequiredService<IIdKeyMap>(),
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<HybridCache>(),
            GetRequiredService<IPublishedContentFactory>());

        _mockedCache = new ContentCache(_mockContentCacheService, GetRequiredService<IIdKeyMap>());
    }

    [Test]
    public async Task Content_Is_Cached_By_Key()
    {
        var textPage = await _mockedCache.GetById(Textpage.Key, true);
        var textPage2 = await _mockedCache.GetById(Textpage.Key, true);
        AssertTextPage(textPage);
        AssertTextPage(textPage2);
        _mockedNucacheRepository.Verify(x => x.GetContentSource(It.IsAny<int>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Cached_By_Id()
    {
        var textPage = await _mockedCache.GetById(Textpage.Id, true);
        var textPage2 = await _mockedCache.GetById(Textpage.Id, true);
        AssertTextPage(textPage);
        AssertTextPage(textPage2);
        _mockedNucacheRepository.Verify(x => x.GetContentSource(It.IsAny<int>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Seeded_By_ContentType_By_Id()
    {
        await _mockContentCacheService.DeleteItemAsync(Textpage.Id);

        await _mockContentCacheService.SeedAsync(new [] {Textpage.ContentTypeId});
        var textPage = await _mockedCache.GetById(Textpage.Id, true);
        AssertTextPage(textPage);

        _mockedNucacheRepository.Verify(x => x.GetContentSource(It.IsAny<int>()), Times.Exactly(0));
    }

    [Test]
    public async Task Content_Is_Seeded_By_ContentType_By_Key()
    {
        await _mockContentCacheService.DeleteItemAsync(Textpage.Id);

        await _mockContentCacheService.SeedAsync(new [] {Textpage.ContentTypeId});
        var textPage = await _mockedCache.GetById(Textpage.Key, true);
        AssertTextPage(textPage);

        _mockedNucacheRepository.Verify(x => x.GetContentSource(It.IsAny<int>()), Times.Exactly(0));
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
