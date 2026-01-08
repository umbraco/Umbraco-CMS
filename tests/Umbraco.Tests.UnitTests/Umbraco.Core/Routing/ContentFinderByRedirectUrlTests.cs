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

    [Test]
    public async Task Domain_Specific_Redirect_Takes_Priority_Over_Path_Only_Redirect()
    {
        const string FullPath = "/en/old-page-path";
        const string DomainRelativePath = "/old-page-path";
        const int PathOnlyRedirectContentId = 9999;
        const string PathOnlyRedirectNewPath = "/wrong-new-path";
        const string DomainRedirectNewPath = "/correct-new-path";

        var domainPrefixedOldPath = $"{DomainContentId}{DomainRelativePath}";

        var mockRedirectUrlService = new Mock<IRedirectUrlService>();
        mockRedirectUrlService
            .Setup(x => x.GetMostRecentRedirectUrlAsync(It.Is<string>(y => y == domainPrefixedOldPath), It.IsAny<string>()))
            .ReturnsAsync(new RedirectUrl { ContentId = ContentId });
        mockRedirectUrlService
            .Setup(x => x.GetMostRecentRedirectUrlAsync(It.Is<string>(y => y == FullPath), It.IsAny<string>()))
            .ReturnsAsync(new RedirectUrl { ContentId = PathOnlyRedirectContentId });

        var mockDomainContent = new Mock<IPublishedContent>();
        mockDomainContent.SetupGet(x => x.Id).Returns(ContentId);
        mockDomainContent.SetupGet(x => x.ContentType.ItemType).Returns(PublishedItemType.Content);

        var mockPathOnlyContent = new Mock<IPublishedContent>();
        mockPathOnlyContent.SetupGet(x => x.Id).Returns(PathOnlyRedirectContentId);
        mockPathOnlyContent.SetupGet(x => x.ContentType.ItemType).Returns(PublishedItemType.Content);

        var mockUmbracoContext = new Mock<IUmbracoContext>();
        mockUmbracoContext
            .Setup(x => x.Content.GetById(It.Is<int>(y => y == ContentId)))
            .Returns(mockDomainContent.Object);
        mockUmbracoContext
            .Setup(x => x.Content.GetById(It.Is<int>(y => y == PathOnlyRedirectContentId)))
            .Returns(mockPathOnlyContent.Object);

        var mockUmbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = mockUmbracoContext.Object;
        mockUmbracoContextAccessor
            .Setup(x => x.TryGetUmbracoContext(out umbracoContext))
            .Returns(true);

        var mockPublishedUrlProvider = new Mock<IPublishedUrlProvider>();
        mockPublishedUrlProvider
            .Setup(x => x.GetUrl(It.Is<IPublishedContent>(y => y.Id == ContentId), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(DomainRedirectNewPath);
        mockPublishedUrlProvider
            .Setup(x => x.GetUrl(It.Is<IPublishedContent>(y => y.Id == PathOnlyRedirectContentId), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(PathOnlyRedirectNewPath);

        var sut = CreateContentFinder(mockRedirectUrlService, mockUmbracoContextAccessor, mockPublishedUrlProvider);
        var publishedRequestBuilder = CreatePublishedRequestBuilder(FullPath, withDomain: true);

        var result = await sut.TryFindContent(publishedRequestBuilder);

        Assert.That(result, Is.True);
        Assert.That((HttpStatusCode)publishedRequestBuilder.ResponseStatusCode, Is.EqualTo(HttpStatusCode.Moved));
        mockRedirectUrlService.Verify(
            x => x.GetMostRecentRedirectUrlAsync(domainPrefixedOldPath, It.IsAny<string>()),
            Times.Once);
        mockRedirectUrlService.Verify(
            x => x.GetMostRecentRedirectUrlAsync(FullPath, It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Falls_Back_To_Path_Only_Redirect_When_No_Domain_Specific_Exists()
    {
        const string FullPath = "/en/old-page-path";
        const string DomainRelativePath = "/old-page-path";
        const string NewPath = "/new-page-path";

        var domainPrefixedOldPath = $"{DomainContentId}{DomainRelativePath}";

        var mockRedirectUrlService = new Mock<IRedirectUrlService>();
        mockRedirectUrlService
            .Setup(x => x.GetMostRecentRedirectUrlAsync(It.Is<string>(y => y == domainPrefixedOldPath), It.IsAny<string>()))
            .ReturnsAsync((IRedirectUrl?)null);
        mockRedirectUrlService
            .Setup(x => x.GetMostRecentRedirectUrlAsync(It.Is<string>(y => y == FullPath), It.IsAny<string>()))
            .ReturnsAsync(new RedirectUrl { ContentId = ContentId });

        var mockContent = CreateMockPublishedContent();
        var mockUmbracoContextAccessor = CreateMockUmbracoContextAccessor(mockContent);
        var mockPublishedUrlProvider = CreateMockPublishedUrlProvider(NewPath);

        var sut = CreateContentFinder(mockRedirectUrlService, mockUmbracoContextAccessor, mockPublishedUrlProvider);
        var publishedRequestBuilder = CreatePublishedRequestBuilder(FullPath, withDomain: true);

        var result = await sut.TryFindContent(publishedRequestBuilder);

        Assert.That(result, Is.True);
        Assert.That((HttpStatusCode)publishedRequestBuilder.ResponseStatusCode, Is.EqualTo(HttpStatusCode.Moved));
        mockRedirectUrlService.Verify(
            x => x.GetMostRecentRedirectUrlAsync(domainPrefixedOldPath, It.IsAny<string>()),
            Times.Once);
        mockRedirectUrlService.Verify(
            x => x.GetMostRecentRedirectUrlAsync(FullPath, It.IsAny<string>()),
            Times.Once);
    }

    private static void AssertRedirectResult(PublishedRequestBuilder publishedRequestBuilder, bool result)
    {
        Assert.AreEqual(true, result);
        Assert.AreEqual(HttpStatusCode.Moved, (HttpStatusCode)publishedRequestBuilder.ResponseStatusCode);
    }
}
