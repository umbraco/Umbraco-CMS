using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class UrlProviderTests
{
    private static readonly Uri _currentUri = new("http://localhost", UriKind.Absolute);

    [TestCase(UrlMode.Auto, UrlMode.Auto)]
    [TestCase(UrlMode.Relative, UrlMode.Relative)]
    [TestCase(UrlMode.Absolute, UrlMode.Absolute)]

    // UrlMode.Default defers to the configured mode, so configuring Default leaves nothing to defer
    // to. Providers are contracted to receive a concrete mode, so it must resolve to the shipped one.
    [TestCase(UrlMode.Default, UrlMode.Auto)]
    public void Get_Url_Passes_A_Concrete_Mode_To_Providers(UrlMode configuredMode, UrlMode expectedMode)
    {
        Mock<IUrlProvider> urlProvider = CreateUrlProvider();
        UrlProvider sut = CreateSut(configuredMode, urlProvider);

        sut.GetUrl(CreateContent(), UrlMode.Default, "en-US", _currentUri);

        urlProvider.Verify(
            x => x.GetUrl(It.IsAny<IPublishedContent>(), expectedMode, It.IsAny<string>(), It.IsAny<Uri>()),
            Times.Once);
    }

    [Test]
    public void Get_Url_Prefers_An_Explicitly_Requested_Mode_Over_The_Configured_One()
    {
        Mock<IUrlProvider> urlProvider = CreateUrlProvider();
        UrlProvider sut = CreateSut(UrlMode.Relative, urlProvider);

        sut.GetUrl(CreateContent(), UrlMode.Absolute, "en-US", _currentUri);

        urlProvider.Verify(
            x => x.GetUrl(It.IsAny<IPublishedContent>(), UrlMode.Absolute, It.IsAny<string>(), It.IsAny<Uri>()),
            Times.Once);
    }

    private static Mock<IUrlProvider> CreateUrlProvider()
    {
        var urlProvider = new Mock<IUrlProvider>();
        urlProvider
            .Setup(x => x.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
            .Returns(UrlInfo.AsUrl("/some/where", "test"));
        return urlProvider;
    }

    private static IPublishedContent CreateContent()
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.Setup(x => x.ItemType).Returns(PublishedItemType.Content);

        var content = new Mock<IPublishedContent>();
        content.Setup(x => x.ContentType).Returns(contentType.Object);
        return content.Object;
    }

    private static UrlProvider CreateSut(UrlMode configuredMode, Mock<IUrlProvider> urlProvider)
        => new(
            Mock.Of<IUmbracoContextAccessor>(),
            Options.Create(new WebRoutingSettings { UrlProviderMode = configuredMode }),
            new UrlProviderCollection(() => [urlProvider.Object]),
            new MediaUrlProviderCollection(() => []),
            Mock.Of<IVariationContextAccessor>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IPublishedContentStatusFilteringService>());
}
