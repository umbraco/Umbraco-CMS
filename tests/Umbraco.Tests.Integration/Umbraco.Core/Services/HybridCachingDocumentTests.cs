using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class HybridCachingDocumentTests : UmbracoIntegrationTestWithContent
{
    private IPublishedHybridCache _mockedCache;
    private Mock<ICacheService> _mockedCacheService;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddSingleton<IPublishedHybridCache, ContentCache>();
        services.AddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IContentCacheDataSerializerFactory, MsgPackContentNestedDataSerializerFactory>();
        services.AddSingleton<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();
        services.AddNotificationAsyncHandler<ContentRefreshNotification, CacheRefreshingNotificationHandler>();
        services.AddTransient<IPublishedContentFactory, PublishedContentFactory>();
    }

    [SetUp]
    public void SetUp()
    {
        _mockedCacheService = new Mock<ICacheService>();

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
        _mockedCacheService.Setup(r => r.GetById(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(
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

        _mockedCacheService.Setup(x => x.GetByKey(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(
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

        _mockedCache = new ContentCache(
            GetRequiredService<HybridCache>(),
            _mockedCacheService.Object,
            GetRequiredService<IPublishedContentFactory>());
    }

    private IPublishedHybridCache PublishedHybridCache => GetRequiredService<IPublishedHybridCache>();

    [Test]
    public async Task Can_Get_Unpublished_Content_By_Id()
    {
        var textPage = await PublishedHybridCache.GetById(Textpage.Id, true);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Unpublished_Content_By_Key()
    {
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, true);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Id()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var textPage = await PublishedHybridCache.GetById(Textpage.Id);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Can_Get_Published_Content_By_Key()
    {
        ContentService.Publish(Textpage, Array.Empty<string>());
        var textPage = await PublishedHybridCache.GetById(Textpage.Key);
        AssertTextPage(textPage);
    }

    [Test]
    public async Task Content_Is_Cached_By_Key()
    {
        var textPage = await _mockedCache.GetById(Textpage.Key, true);
        var textPage2 = await _mockedCache.GetById(Textpage.Key, true);
        AssertTextPage(textPage);
        AssertTextPage(textPage2);
        _mockedCacheService.Verify(x => x.GetByKey(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Cached_By_Id()
    {
        var textPage = await _mockedCache.GetById(Textpage.Id, true);
        var textPage2 = await _mockedCache.GetById(Textpage.Id, true);
        AssertTextPage(textPage);
        AssertTextPage(textPage2);
        _mockedCacheService.Verify(x => x.GetById(It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(1));
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
