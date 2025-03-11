using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

public class PropertyValueConverterTests : DeliveryApiTests
{
    protected ICacheManager CacheManager { get; private set; }

    protected IPublishedUrlProvider PublishedUrlProvider { get; private set; }

    protected IApiContentPathProvider ApiContentPathProvider { get; private set; }

    protected IPublishedContent PublishedContent { get; private set; }

    protected IPublishedContent PublishedMedia { get; private set; }

    protected IPublishedContentType PublishedContentType { get; private set; }

    protected IPublishedContentType PublishedMediaType { get; private set; }

    protected Mock<IPublishedContentCache> PublishedContentCacheMock { get; private set; }

    protected Mock<IPublishedMediaCache> PublishedMediaCacheMock { get; private set; }

    protected Mock<IPublishedUrlProvider> PublishedUrlProviderMock { get; private set; }

    protected VariationContext VariationContext { get; } = new();

    protected Mock<IDocumentNavigationQueryService> DocumentNavigationQueryServiceMock { get; private set; }

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var publishedContentType = new Mock<IPublishedContentType>();
        publishedContentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        publishedContentType.SetupGet(c => c.Alias).Returns("TheContentType");
        PublishedContentType = publishedContentType.Object;
        var publishedMediaType = new Mock<IPublishedContentType>();
        publishedMediaType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);
        publishedMediaType.SetupGet(c => c.Alias).Returns("TheMediaType");
        PublishedMediaType = publishedMediaType.Object;

        var contentKey = Guid.NewGuid();
        var publishedContent = SetupPublishedContent("The page", contentKey, PublishedItemType.Content, publishedContentType.Object);
        PublishedContent = publishedContent.Object;

        var mediaKey = Guid.NewGuid();
        var publishedMedia = SetupPublishedContent("The media", mediaKey, PublishedItemType.Media, publishedMediaType.Object);
        PublishedMedia = publishedMedia.Object;

        PublishedContentCacheMock = new Mock<IPublishedContentCache>();
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(contentKey))
            .Returns(publishedContent.Object);
        PublishedMediaCacheMock = new Mock<IPublishedMediaCache>();
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(mediaKey))
            .Returns(publishedMedia.Object);

        var cacheMock = new Mock<ICacheManager>();
        cacheMock.SetupGet(cache => cache.Content).Returns(PublishedContentCacheMock.Object);
        cacheMock.SetupGet(cache => cache.Media).Returns(PublishedMediaCacheMock.Object);
        CacheManager = cacheMock.Object;

        PublishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(publishedContent.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns("the-page-url");
        PublishedUrlProviderMock
            .Setup(p => p.GetMediaUrl(publishedMedia.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns("the-media-url");
        PublishedUrlProvider = PublishedUrlProviderMock.Object;
        ApiContentPathProvider = new ApiContentPathProvider(PublishedUrlProvider);

        DocumentNavigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        IEnumerable<Guid> ancestorsKeys = [];
        DocumentNavigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(contentKey, out ancestorsKeys)).Returns(true);
    }

    protected Mock<IPublishedContent> SetupPublishedContent(string name, Guid key, PublishedItemType itemType, IPublishedContentType contentType)
    {
        var content = new Mock<IPublishedContent>();
        var urlSegment = "url-segment";
        ConfigurePublishedContentMock(content, key, name, urlSegment, contentType, Array.Empty<PublishedElementPropertyBase>());
        content.SetupGet(c => c.ItemType).Returns(itemType);
        return content;
    }

    protected void RegisterContentWithProviders(IPublishedContent content)
    {
        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(content, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(content.UrlSegment);
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(content.Key))
            .Returns(content);
    }

    protected void RegisterMediaWithProviders(IPublishedContent media)
    {
        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(media, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(media.UrlSegment);
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(media.Key))
            .Returns(media);
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(It.IsAny<bool>(), media.Key))
            .Returns(media);
    }

    protected override ApiContentRouteBuilder CreateContentRouteBuilder(
        IApiContentPathProvider contentPathProvider,
        IOptions<GlobalSettings> globalSettings,
        IVariationContextAccessor? variationContextAccessor = null,
        IRequestPreviewService? requestPreviewService = null,
        IOptionsMonitor<RequestHandlerSettings>? requestHandlerSettingsMonitor = null,
        IPublishedContentCache? contentCache = null,
        IDocumentNavigationQueryService? navigationQueryService = null,
        IPublishStatusQueryService? publishStatusQueryService = null)
    {
        contentCache ??= PublishedContentCacheMock.Object;
        navigationQueryService ??= DocumentNavigationQueryServiceMock.Object;

        return base.CreateContentRouteBuilder(
            contentPathProvider,
            globalSettings,
            variationContextAccessor,
            requestPreviewService,
            requestHandlerSettingsMonitor,
            contentCache,
            navigationQueryService,
            publishStatusQueryService);
    }
}
