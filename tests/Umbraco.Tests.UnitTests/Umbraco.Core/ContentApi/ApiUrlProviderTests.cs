using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class ApiUrlProviderTests : PropertyValueConverterTests
{
    [TestCase("/some/url/for/the/page", null, "/some/url/for/the/page")]
    [TestCase("/some/url", "home-root", "/some/url")]
    [TestCase("/root/some/url", "root", "/some/url")]
    [TestCase("/root/some/url", "root-two", "/root/some/url")]
    [TestCase("/root-two/some/url", "root-two", "/some/url")]
    [TestCase("/", "root", "/")]
    [TestCase("/", null, "/")]
    public void Can_Handle_Request_Start_Node_Path_In_Content_Url(string publishedUrl, string? requestStartNodePath, string expectedUrl)
    {
        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(content.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(publishedUrl);

        var apiUrlProvider = new ApiUrlProvider(publishedUrlProvider.Object, CreateStartNodeServiceAccessor(requestStartNodePath));
        var result = apiUrlProvider.Url(content.Object);
        Assert.AreEqual(expectedUrl, result);
    }

    [TestCase("/some/url/for/the/media.jpg", null)]
    [TestCase("/some/media.url", "home-root")]
    [TestCase("/root/some/media.url", "root")]
    [TestCase("/root-two/some/media.url", "root-two")]
    [TestCase("/media.url", "root")]
    [TestCase("/media.url", null)]
    public void Does_Not_Apply_Start_Node_Path_For_Media_Url(string publishedUrl, string? requestStartNodePath)
    {
        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetMediaUrl(content.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(publishedUrl);

        var apiUrlProvider = new ApiUrlProvider(publishedUrlProvider.Object, CreateStartNodeServiceAccessor(requestStartNodePath));
        var result = apiUrlProvider.Url(content.Object);
        Assert.AreEqual(publishedUrl, result);
    }

    private IRequestStartNodeServiceAccessor CreateStartNodeServiceAccessor(string? requestStartNodePath = null)
    {
        var serviceMock = new Mock<IRequestStartNodeService>();
        serviceMock.Setup(m => m.GetRequestedStartNodePath()).Returns(requestStartNodePath);

        var service = serviceMock.Object;
        var accessorMock = new Mock<IRequestStartNodeServiceAccessor>();
        accessorMock.Setup(m => m.TryGetValue(out service)).Returns(true);
        return accessorMock.Object;
    }
}
