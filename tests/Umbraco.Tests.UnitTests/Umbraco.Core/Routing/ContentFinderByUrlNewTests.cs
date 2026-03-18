using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

/// <summary>
/// Contains unit tests for the <see cref="ContentFinderByUrlNew"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class ContentFinderByUrlNewTests
{
    private const int DomainContentId = 1233;
    private const int ContentId = 1234;
    private static readonly Guid _contentKey = Guid.NewGuid();
    private const string ContentPath = "/test-page";
    private const string DomainHost = "example.com";

    /// <summary>
    /// Tests whether the content finder can locate invariant (culture-neutral) content based on the provided URL path.
    /// </summary>
    /// <param name="path">The URL path to attempt to resolve to content.</param>
    /// <param name="expectSuccess">True if the content is expected to be found; otherwise, false.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestCase(ContentPath, true)]
    [TestCase("/missing-page", false)]
    public async Task Can_Find_Invariant_Content(string path, bool expectSuccess)
    {
        var mockContent = CreateMockPublishedContent();

        var mockUmbracoContextAccessor = CreateMockUmbracoContextAccessor();

        var mockDocumentUrlService = CreateMockDocumentUrlService();

        var mockPublishedContentCache = CreateMockPublishedContentCache(mockContent);

        var sut = CreateContentFinder(mockUmbracoContextAccessor, mockDocumentUrlService, mockPublishedContentCache);

        var publishedRequestBuilder = CreatePublishedRequestBuilder(path);

        var result = await sut.TryFindContent(publishedRequestBuilder);

        Assert.AreEqual(expectSuccess, result);
        if (expectSuccess)
        {
            Assert.IsNotNull(publishedRequestBuilder.PublishedContent);
        }
        else
        {
            Assert.IsNull(publishedRequestBuilder.PublishedContent);
        }
    }

    /// <summary>
    /// Verifies that invariant (culture-neutral) content can be found or not found via URL resolution when a domain is set or not set, and when strict domain matching is enabled or disabled.
    /// </summary>
    /// <param name="path">The URL path to resolve content for.</param>
    /// <param name="setDomain">If true, associates a domain with the request; otherwise, no domain is set.</param>
    /// <param name="useStrictDomainMatching">If true, enables strict domain matching in routing settings.</param>
    /// <param name="expectSuccess">True if content resolution is expected to succeed; otherwise, false.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestCase(ContentPath, true, false, true)]
    [TestCase("/missing-page", true, false, false)]
    [TestCase(ContentPath, true, true, true)]
    [TestCase(ContentPath, false, true, false)]
    public async Task Can_Find_Invariant_Content_With_Domain(string path, bool setDomain, bool useStrictDomainMatching, bool expectSuccess)
    {
        var mockContent = CreateMockPublishedContent();

        var mockUmbracoContextAccessor = CreateMockUmbracoContextAccessor();

        var mockDocumentUrlService = CreateMockDocumentUrlService();

        var mockPublishedContentCache = CreateMockPublishedContentCache(mockContent);

        var sut = CreateContentFinder(
            mockUmbracoContextAccessor,
            mockDocumentUrlService,
            mockPublishedContentCache,
            new WebRoutingSettings
            {
                UseStrictDomainMatching = useStrictDomainMatching
            });

        var publishedRequestBuilder = CreatePublishedRequestBuilder(path, withDomain: setDomain);

        var result = await sut.TryFindContent(publishedRequestBuilder);

        Assert.AreEqual(expectSuccess, result);
        if (expectSuccess)
        {
            Assert.IsNotNull(publishedRequestBuilder.PublishedContent);
        }
        else
        {
            Assert.IsNull(publishedRequestBuilder.PublishedContent);
        }
    }

    private static Mock<IPublishedContent> CreateMockPublishedContent()
    {
        var mockContent = new Mock<IPublishedContent>();
        mockContent
            .SetupGet(x => x.Id)
            .Returns(ContentId);
        mockContent
            .SetupGet(x => x.ContentType.ItemType)
            .Returns(PublishedItemType.Content);
        return mockContent;
    }

    private static Mock<IUmbracoContextAccessor> CreateMockUmbracoContextAccessor()
    {
        var mockUmbracoContext = new Mock<IUmbracoContext>();
        var mockUmbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = mockUmbracoContext.Object;
        mockUmbracoContextAccessor
            .Setup(x => x.TryGetUmbracoContext(out umbracoContext))
            .Returns(true);
        return mockUmbracoContextAccessor;
    }

    private static Mock<IDocumentUrlService> CreateMockDocumentUrlService()
    {
        var mockDocumentUrlService = new Mock<IDocumentUrlService>();
        mockDocumentUrlService
            .Setup(x => x.GetDocumentKeyByRoute(It.Is<string>(y => y == ContentPath), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns(_contentKey);
        return mockDocumentUrlService;
    }

    private static Mock<IPublishedContentCache> CreateMockPublishedContentCache(Mock<IPublishedContent> mockContent)
    {
        var mockPublishedContentCache = new Mock<IPublishedContentCache>();
        mockPublishedContentCache
            .Setup(x => x.GetById(It.IsAny<bool>(), It.Is<Guid>(y => y == _contentKey)))
            .Returns(mockContent.Object);
        return mockPublishedContentCache;
    }

    private static ContentFinderByUrlNew CreateContentFinder(
        Mock<IUmbracoContextAccessor> mockUmbracoContextAccessor,
        Mock<IDocumentUrlService> mockDocumentUrlService,
        Mock<IPublishedContentCache> mockPublishedContentCache,
        WebRoutingSettings? webRoutingSettings = null)
        => new(
            new NullLogger<ContentFinderByUrlNew>(),
            mockUmbracoContextAccessor.Object,
            mockDocumentUrlService.Object,
            mockPublishedContentCache.Object,
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == (webRoutingSettings ?? new WebRoutingSettings())));

    private static PublishedRequestBuilder CreatePublishedRequestBuilder(string path, bool withDomain = false)
    {
        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri($"https://example.com{path}"), Mock.Of<IFileService>());
        if (withDomain)
        {
            publishedRequestBuilder.SetDomain(new DomainAndUri(new Domain(1, $"https://{DomainHost}/", DomainContentId, "en-US", false, 0), new Uri($"https://{DomainHost}{path}")));
        }

        return publishedRequestBuilder;
    }
}
