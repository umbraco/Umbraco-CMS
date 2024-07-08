using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class HybridCachingTests : UmbracoIntegrationTestWithContent
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
            GetRequiredService<IPublishedSnapshotAccessor>(),
            GetRequiredService<IVariationContextAccessor>(),
            GetRequiredService<IPublishedModelFactory>(),
            GetRequiredService<IContentTypeService>(),
            GetRequiredService<IPublishedContentTypeFactory>(),
            GetRequiredService<ILoggerFactory>());
    }

    private IPublishedHybridCache PublishedHybridCache => GetRequiredService<IPublishedHybridCache>();

    [Test]
    public async Task Can_Get_Content_By_Id()
    {
        var textPage = await PublishedHybridCache.GetById(Textpage.Id, true);

        Assert.IsNotNull(textPage);
    }

    [Test]
    public async Task Can_Get_Content_By_Key()
    {
        var textPage = await PublishedHybridCache.GetById(Textpage.Key, true);

        Assert.IsNotNull(textPage);
    }

    [Test]
    public async Task Content_Is_Cached_By_Key()
    {
        var textPage = await _mockedCache.GetById(Textpage.Key, true);
        var textPage2 = await _mockedCache.GetById(Textpage.Key, true);
        Assert.IsNotNull(textPage);
        Assert.IsNotNull(textPage2);

        _mockedCacheService.Verify(x => x.GetByKey(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(1));
    }

    [Test]
    public async Task Content_Is_Cached_By_Id()
    {
        var textPage = await _mockedCache.GetById(Textpage.Id, true);
        var textPage2 = await _mockedCache.GetById(Textpage.Id, true);
        Assert.IsNotNull(textPage);
        Assert.IsNotNull(textPage2);

        _mockedCacheService.Verify(x => x.GetById(It.IsAny<int>(), It.IsAny<bool>()), Times.Exactly(1));
    }
}
