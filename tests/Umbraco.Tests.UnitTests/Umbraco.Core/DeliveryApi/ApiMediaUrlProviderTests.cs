using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ApiMediaUrlProviderTests : PropertyValueConverterTests
{
    [TestCase("/some/url/for/the/media.jpg")]
    [TestCase("/some/media.url")]
    [TestCase("/root/some/media.url")]
    [TestCase("/root-two/some/media.url")]
    [TestCase("/media.url")]
    public void Media_Url_Provider_Returns_Relative_Published_Media_Url(string publishedUrl)
    {
        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetMediaUrl(content.Object, UrlMode.Default, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(publishedUrl);

        var apiMediaUrlProvider = new ApiMediaUrlProvider(publishedUrlProvider.Object);
        var result = apiMediaUrlProvider.GetUrl(content.Object);
        Assert.AreEqual(publishedUrl, result);
    }

    [TestCase(PublishedItemType.Content)]
    [TestCase(PublishedItemType.Element)]
    [TestCase(PublishedItemType.Member)]
    [TestCase(PublishedItemType.Unknown)]
    public void Does_Not_Support_Non_Media_Types(PublishedItemType itemType)
    {
        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ItemType).Returns(itemType);

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(content.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns("somewhere.else");

        var apiUrlProvider = new ApiMediaUrlProvider(publishedUrlProvider.Object);
        Assert.Throws<ArgumentException>(() => apiUrlProvider.GetUrl(content.Object));
    }
}
