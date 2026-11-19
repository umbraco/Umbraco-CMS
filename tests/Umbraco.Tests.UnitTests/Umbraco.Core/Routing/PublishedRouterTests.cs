using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class PublishedRouterTests
{
    private PublishedRouter CreatePublishedRouter(
        IUmbracoContextAccessor umbracoContextAccessor,
        IPublishStatusQueryService? publishStatusQueryService = null,
        IIdKeyMap? idKeyMap = null,
        IPreviewService? previewService = null)
        => new(
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == new WebRoutingSettings()),
            new ContentFinderCollection(() => Enumerable.Empty<IContentFinder>()),
            new TestLastChanceFinder(),
            new TestVariationContextAccessor(),
            Mock.Of<IProfilingLogger>(),
            Mock.Of<ILogger<PublishedRouter>>(),
            Mock.Of<IPublishedUrlProvider>(),
            Mock.Of<IRequestAccessor>(),
            Mock.Of<IPublishedValueFallback>(),
            Mock.Of<IFileService>(),
            Mock.Of<IContentTypeService>(),
            umbracoContextAccessor,
            Mock.Of<IEventAggregator>(),
            Mock.Of<IDomainCache>(),
            publishStatusQueryService ?? Mock.Of<IPublishStatusQueryService>(),
            idKeyMap ?? Mock.Of<IIdKeyMap>(),
            previewService ?? Mock.Of<IPreviewService>());

    private IUmbracoContextAccessor GetUmbracoContextAccessor()
    {
        var uri = new Uri("http://example.com");
        var umbracoContext = Mock.Of<IUmbracoContext>(x => x.CleanedUmbracoUrl == uri);
        var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
        return umbracoContextAccessor;
    }

    [Test]
    public async Task ConfigureRequest_Returns_False_Without_HasPublishedContent()
    {
        var umbracoContextAccessor = GetUmbracoContextAccessor();
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var request =
            await publishedRouter.CreateRequestAsync(umbracoContextAccessor.GetRequiredUmbracoContext()
                .CleanedUmbracoUrl);
        var result = publishedRouter.BuildRequest(request);

        Assert.IsFalse(result.Success());
    }

    [Test]
    public async Task ConfigureRequest_Returns_False_When_IsRedirect()
    {
        var umbracoContextAccessor = GetUmbracoContextAccessor();
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var request =
            await publishedRouter.CreateRequestAsync(umbracoContextAccessor.GetRequiredUmbracoContext()
                .CleanedUmbracoUrl);
        var content = GetPublishedContentMock();
        request.SetPublishedContent(content.Object);
        request.SetCulture("en-AU");
        request.SetRedirect("/hello");
        var result = publishedRouter.BuildRequest(request);

        Assert.IsFalse(result.Success());
    }

    [Test]
    public async Task FindAndSetDomain_Resolves_Published_Domain_Without_Reading_The_Content_Cache()
    {
        (PublishedRouter router, IPublishedRequestBuilder request, _) = await SetUpDomainRoutingAsync(
            domainContentId: 1234,
            domainCulture: "da-DK",
            isPublishedInCulture: true,
            hasPublishedAncestorPath: true,
            isInPreview: false);

        Assert.IsTrue(router.FindAndSetDomain(request));
        Assert.AreEqual("da-DK", request.Domain?.Culture);
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(false, false)]
    public async Task FindAndSetDomain_Ignores_Domain_On_Content_That_Is_Not_Published(bool isPublishedInCulture, bool hasPublishedAncestorPath)
    {
        (PublishedRouter router, IPublishedRequestBuilder request, _) = await SetUpDomainRoutingAsync(
            domainContentId: 1234,
            domainCulture: "da-DK",
            isPublishedInCulture: isPublishedInCulture,
            hasPublishedAncestorPath: hasPublishedAncestorPath,
            isInPreview: false);

        Assert.IsFalse(router.FindAndSetDomain(request));
    }

    [Test]
    public async Task FindAndSetDomain_Ignores_Domain_On_Content_That_Cannot_Be_Resolved()
    {
        (PublishedRouter router, IPublishedRequestBuilder request, _) = await SetUpDomainRoutingAsync(
            domainContentId: 1234,
            domainCulture: "da-DK",
            isPublishedInCulture: true,
            hasPublishedAncestorPath: true,
            isInPreview: false,
            keyResolves: false);

        Assert.IsFalse(router.FindAndSetDomain(request));
    }

    [Test]
    public async Task FindAndSetDomain_Resolves_Domain_From_The_Content_Cache_When_Previewing()
    {
        (PublishedRouter router, IPublishedRequestBuilder request, Mock<IPublishedContentCache> contentCache) = await SetUpDomainRoutingAsync(
            domainContentId: 1234,
            domainCulture: "da-DK",
            isPublishedInCulture: false,
            hasPublishedAncestorPath: false,
            isInPreview: true);

        Assert.IsTrue(router.FindAndSetDomain(request));
        contentCache.Verify(x => x.GetById(1234), Times.Once);
    }

    [Test]
    public async Task FindAndSetDomain_Takes_The_Preview_Decision_From_The_Preview_Service_Not_The_Umbraco_Context()
    {
        // The content cache reads drafts when the preview service says so, regardless of the Umbraco context flag.
        (PublishedRouter router, IPublishedRequestBuilder request, Mock<IPublishedContentCache> contentCache) = await SetUpDomainRoutingAsync(
            domainContentId: 1234,
            domainCulture: "da-DK",
            isPublishedInCulture: false,
            hasPublishedAncestorPath: false,
            isInPreview: true,
            umbracoContextInPreviewMode: false);

        Assert.IsTrue(router.FindAndSetDomain(request));
        contentCache.Verify(x => x.GetById(1234), Times.Once);
    }

    [Test]
    public async Task FindAndSetDomain_Ignores_The_Umbraco_Context_Preview_Flag_When_The_Preview_Service_Says_No()
    {
        // Strict content cache: reading it would fail the test.
        (PublishedRouter router, IPublishedRequestBuilder request, _) = await SetUpDomainRoutingAsync(
            domainContentId: 1234,
            domainCulture: "da-DK",
            isPublishedInCulture: true,
            hasPublishedAncestorPath: true,
            isInPreview: false,
            umbracoContextInPreviewMode: true);

        Assert.IsTrue(router.FindAndSetDomain(request));
    }

    private async Task<(PublishedRouter Router, IPublishedRequestBuilder Request, Mock<IPublishedContentCache> ContentCache)> SetUpDomainRoutingAsync(
        int domainContentId,
        string domainCulture,
        bool isPublishedInCulture,
        bool hasPublishedAncestorPath,
        bool isInPreview,
        bool keyResolves = true,
        bool umbracoContextInPreviewMode = false)
    {
        var documentKey = Guid.NewGuid();

        var idKeyMap = new Mock<IIdKeyMap>();
        idKeyMap
            .Setup(x => x.GetKeyForId(domainContentId, UmbracoObjectTypes.Document))
            .Returns(keyResolves ? Attempt.Succeed(documentKey) : Attempt<Guid>.Fail());

        var publishStatus = new Mock<IPublishStatusQueryService>();
        publishStatus.Setup(x => x.IsDocumentPublished(documentKey, domainCulture)).Returns(isPublishedInCulture);
        publishStatus.Setup(x => x.HasPublishedAncestorPath(documentKey)).Returns(hasPublishedAncestorPath);

        // Strict, so that reading content from the cache outside of preview fails the test.
        var contentCache = new Mock<IPublishedContentCache>(MockBehavior.Strict);
        if (isInPreview)
        {
            var domainDocument = new Mock<IPublishedContent>();
            domainDocument.Setup(x => x.ContentType).Returns(
                new PublishedContentType(Guid.NewGuid(), 22, "anything", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing));
            contentCache.Setup(x => x.GetById(domainContentId)).Returns(domainDocument.Object);
        }

        var domainCache = new Mock<IDomainCache>();
        domainCache.Setup(x => x.GetAll(false)).Returns([new Domain(1, "example.com", domainContentId, domainCulture, false, 0)]);
        domainCache.Setup(x => x.DefaultCulture).Returns(domainCulture);

        var uri = new Uri("http://example.com");
        var umbracoContext = new Mock<IUmbracoContext>();
        umbracoContext.Setup(x => x.CleanedUmbracoUrl).Returns(uri);
        umbracoContext.Setup(x => x.Domains).Returns(domainCache.Object);
        umbracoContext.Setup(x => x.Content).Returns(contentCache.Object);
        umbracoContext.Setup(x => x.InPreviewMode).Returns(umbracoContextInPreviewMode);

        var previewService = new Mock<IPreviewService>();
        previewService.Setup(x => x.IsInPreview()).Returns(isInPreview);

        var accessor = new TestUmbracoContextAccessor(umbracoContext.Object);
        PublishedRouter router = CreatePublishedRouter(accessor, publishStatus.Object, idKeyMap.Object, previewService: previewService.Object);
        IPublishedRequestBuilder request = await router.CreateRequestAsync(uri);
        return (router, request, contentCache);
    }

    private Mock<IPublishedContent> GetPublishedContentMock()
    {
        var pc = new Mock<IPublishedContent>();
        pc.Setup(content => content.Id).Returns(1);
        pc.Setup(content => content.Name).Returns("test");
        pc.Setup(content => content.CreateDate).Returns(DateTime.UtcNow);
        pc.Setup(content => content.UpdateDate).Returns(DateTime.UtcNow);
        pc.Setup(content => content.Path).Returns("-1,1");
        pc.Setup(content => content.Properties).Returns(new Collection<IPublishedProperty>());
        pc.Setup(content => content.ContentType)
            .Returns(new PublishedContentType(
                Guid.NewGuid(),
                22,
                "anything",
                PublishedItemType.Content,
                Enumerable.Empty<string>(),
                Enumerable.Empty<PublishedPropertyType>(),
                ContentVariation.Nothing));
        return pc;
    }
}
