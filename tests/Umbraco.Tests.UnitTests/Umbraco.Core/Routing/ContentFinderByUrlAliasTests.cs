using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByUrlAliasTests
{
    [Test]
    [InlineAutoMoqData("/this/is/my/alias", 1001)]
    [InlineAutoMoqData("/anotheralias", 1001)]
    [InlineAutoMoqData("/page2/alias", 10011)]
    [InlineAutoMoqData("/2ndpagealias", 10011)]
    [InlineAutoMoqData("/only/one/alias", 100111)]
    [InlineAutoMoqData("/ONLY/one/Alias", 100111)]
    [InlineAutoMoqData("/alias43", 100121)]
    public async Task Lookup_By_Url_Alias(
        string relativeUrl,
        int nodeMatch,
        [Frozen] IPublishedContentCache publishedContentCache,
        [Frozen] IUmbracoContextAccessor umbracoContextAccessor,
        [Frozen] IUmbracoContext umbracoContext,
        [Frozen] IDocumentNavigationQueryService documentNavigationQueryService,
        [Frozen] IPublishedContentStatusFilteringService publishedContentStatusFilteringService,
        IFileService fileService,
        IPublishedContent[] rootContents,
        IPublishedProperty urlProperty)
    {
        // Arrange
        var absoluteUrl = "http://localhost" + relativeUrl;

        var contentItem = rootContents[0];
        Mock.Get(umbracoContextAccessor).Setup(x => x.TryGetUmbracoContext(out umbracoContext)).Returns(true);
        Mock.Get(umbracoContext).Setup(x => x.Content).Returns(publishedContentCache);
        Mock.Get(publishedContentCache).Setup(x => x.GetAtRoot(null)).Returns(rootContents);
        Mock.Get(contentItem).Setup(x => x.Id).Returns(nodeMatch);
        Mock.Get(contentItem).Setup(x => x.GetProperty(Constants.Conventions.Content.UrlAlias)).Returns(urlProperty);
        Mock.Get(contentItem).Setup(x => x.ItemType).Returns(PublishedItemType.Content);
        Mock.Get(urlProperty).Setup(x => x.GetValue(null, null)).Returns(relativeUrl);

        IEnumerable<Guid> descendantKeys = [];
        Mock.Get(documentNavigationQueryService).Setup(x => x.TryGetDescendantsKeys(It.IsAny<Guid>(), out descendantKeys)).Returns(true);

        Mock.Get(publishedContentStatusFilteringService).Setup(x => x.FilterAvailable(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string?>())).Returns([]);
        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService);

        // Act
        var sut = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            Mock.Of<IPublishedValueFallback>(),
            umbracoContextAccessor,
            documentNavigationQueryService,
            publishedContentStatusFilteringService);
        var result = await sut.TryFindContent(publishedRequestBuilder);

        Assert.IsTrue(result);
        Assert.AreEqual(publishedRequestBuilder.PublishedContent.Id, nodeMatch);
    }
}
