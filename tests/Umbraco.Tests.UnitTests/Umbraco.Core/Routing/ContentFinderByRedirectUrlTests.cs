using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByRedirectUrlTests
{
    private const int DomainContentId = 1233;
    private const int ContentId = 1234;

    [Test]
    public async Task Can_Find_Invariant_Content()
    {
        const string OldPath = "/old-page-path";
        const string NewPath = "/new-page-path";

        var mockRedirectUrlService = CreateMockRedirectUrlService(OldPath);

        var mockContent = CreateMockPublishedContent();

        var mockUmbracoContextAccessor = CreateMockUmbracoContextAccessor(mockContent);

        var mockPublishedUrlProvider = CreateMockPublishedUrlProvider(NewPath);

        var sut = CreateContentFinder(mockRedirectUrlService, mockUmbracoContextAccessor, mockPublishedUrlProvider);

        var publishedRequestBuilder = CreatePublishedRequestBuilder(OldPath);

        var result = await sut.TryFindContent(publishedRequestBuilder);

        AssertRedirectResult(publishedRequestBuilder, result);
    }

    [Test]
    public async Task Can_Find_Variant_Content_With_Path_Root()
    {
        const string OldPath = "/en/old-page-path";
        const string NewPath = "/en/new-page-path";

        var mockRedirectUrlService = CreateMockRedirectUrlService(OldPath);

        var mockContent = CreateMockPublishedContent();

        var mockUmbracoContextAccessor = CreateMockUmbracoContextAccessor(mockContent);

        var mockPublishedUrlProvider = CreateMockPublishedUrlProvider(NewPath);

        var sut = CreateContentFinder(mockRedirectUrlService, mockUmbracoContextAccessor, mockPublishedUrlProvider);

        var publishedRequestBuilder = CreatePublishedRequestBuilder(OldPath, withDomain: true);

        var result = await sut.TryFindContent(publishedRequestBuilder);

        AssertRedirectResult(publishedRequestBuilder, result);
    }

    [Test]
    public async Task Can_Find_Variant_Content_With_Domain_Node_Id_Prefixed_Path()
    {
        const string OldPath = "/en/old-page-path";
        var domainPrefixedOldPath = $"{DomainContentId}/old-page-path";
        const string NewPath = "/en/new-page-path";

        var mockRedirectUrlService = CreateMockRedirectUrlService(domainPrefixedOldPath);

        var mockContent = CreateMockPublishedContent();

        var mockUmbracoContextAccessor = CreateMockUmbracoContextAccessor(mockContent);

        var mockPublishedUrlProvider = CreateMockPublishedUrlProvider(NewPath);

        var sut = CreateContentFinder(mockRedirectUrlService, mockUmbracoContextAccessor, mockPublishedUrlProvider);

        var publishedRequestBuilder = CreatePublishedRequestBuilder(OldPath, withDomain: true);

        var result = await sut.TryFindContent(publishedRequestBuilder);

        AssertRedirectResult(publishedRequestBuilder, result);
    }

    private static Mock<IRedirectUrlService> CreateMockRedirectUrlService(string oldPath)
    {
        var mockRedirectUrlService = new Mock<IRedirectUrlService>();
        mockRedirectUrlService
            .Setup(x => x.GetMostRecentRedirectUrlAsync(It.Is<string>(y => y == oldPath), It.IsAny<string>()))
            .ReturnsAsync(new RedirectUrl
            {
                ContentId = ContentId,
            });
        return mockRedirectUrlService;
    }

    private static Mock<IPublishedUrlProvider> CreateMockPublishedUrlProvider(string newPath)
    {
        var mockPublishedUrlProvider = new Mock<IPublishedUrlProvider>();
        mockPublishedUrlProvider
            .Setup(x => x.GetUrl(It.Is<IPublishedContent>(y => y.Id == ContentId), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(newPath);
        return mockPublishedUrlProvider;
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

    private static Mock<IUmbracoContextAccessor> CreateMockUmbracoContextAccessor(Mock<IPublishedContent> mockContent)
    {
        var mockUmbracoContext = new Mock<IUmbracoContext>();
        mockUmbracoContext
            .Setup(x => x.Content.GetById(It.Is<int>(y => y == ContentId)))
            .Returns(mockContent.Object);
        var mockUmbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = mockUmbracoContext.Object;
        mockUmbracoContextAccessor
            .Setup(x => x.TryGetUmbracoContext(out umbracoContext))
            .Returns(true);
        return mockUmbracoContextAccessor;
    }

    private static ContentFinderByRedirectUrl CreateContentFinder(
        Mock<IRedirectUrlService> mockRedirectUrlService,
        Mock<IUmbracoContextAccessor> mockUmbracoContextAccessor,
        Mock<IPublishedUrlProvider> mockPublishedUrlProvider)
        => new ContentFinderByRedirectUrl(
            mockRedirectUrlService.Object,
            new NullLogger<ContentFinderByRedirectUrl>(),
            mockPublishedUrlProvider.Object,
            mockUmbracoContextAccessor.Object);

    private static PublishedRequestBuilder CreatePublishedRequestBuilder(string path, bool withDomain = false)
    {
        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri($"https://example.com{path}"), Mock.Of<IFileService>());
        if (withDomain)
        {
            publishedRequestBuilder.SetDomain(new DomainAndUri(new Domain(1, "/en", DomainContentId, "en-US", false, 0), new Uri($"https://example.com{path}")));
        }

        return publishedRequestBuilder;
    }

    private static void AssertRedirectResult(PublishedRequestBuilder publishedRequestBuilder, bool result)
    {
        Assert.AreEqual(true, result);
        Assert.AreEqual(HttpStatusCode.Moved, (HttpStatusCode)publishedRequestBuilder.ResponseStatusCode);
    }
}
