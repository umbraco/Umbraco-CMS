using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
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
        [Frozen] IVariationContextAccessor variationContextAccessor,
        IFileService fileService,
        ContentFinderByUrlAlias sut,
        IPublishedContent[] rootContents,
        IPublishedProperty urlProperty)
    {
        // Arrange
        var absoluteUrl = "http://localhost" + relativeUrl;
        var variationContext = new VariationContext();

        var contentItem = rootContents[0];
        Mock.Get(umbracoContextAccessor).Setup(x => x.TryGetUmbracoContext(out umbracoContext)).Returns(true);
        Mock.Get(umbracoContext).Setup(x => x.Content).Returns(publishedContentCache);
        Mock.Get(publishedContentCache).Setup(x => x.GetAtRoot(null)).Returns(rootContents);
        Mock.Get(contentItem).Setup(x => x.Id).Returns(nodeMatch);
        Mock.Get(contentItem).Setup(x => x.GetProperty(Constants.Conventions.Content.UrlAlias)).Returns(urlProperty);
        Mock.Get(urlProperty).Setup(x => x.GetValue(null, null)).Returns(relativeUrl);

        Mock.Get(variationContextAccessor).Setup(x => x.VariationContext).Returns(variationContext);
        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService);

        // Act
        var result = await sut.TryFindContent(publishedRequestBuilder);

        Assert.IsTrue(result);
        Assert.AreEqual(publishedRequestBuilder.PublishedContent.Id, nodeMatch);
    }
}
