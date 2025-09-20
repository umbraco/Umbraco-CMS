using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ApiContentPathResolverTests
{
    private const string TestPath = "/test/page";

    [TestCase(TestPath, true)]
    [TestCase("/", true)]
    [TestCase("file.txt", false)]
    [TestCase("test/file.txt", false)]
    [TestCase("test/test2/file.txt", false)]
    [TestCase("/file.txt", false)]
    [TestCase("/test/file.txt", false)]
    [TestCase("/test/test2/file.txt", false)]
    public void Can_Verify_Resolveable_Paths(string path, bool expected)
    {
        var resolver = CreateResolver();
        var result = resolver.IsResolvablePath(path);
        Assert.AreEqual(expected, result);
    }

    [TestCase(TestPath)]
    [TestCase("/")]
    public void Resolves_Content_For_Path(string path)
    {
        var resolver = CreateResolver();
        var result = resolver.ResolveContentPath(path);
        Assert.IsNotNull(result);
    }

    private static ApiContentPathResolver CreateResolver()
    {
        var mockRequestRoutingService = new Mock<IRequestRoutingService>();
        mockRequestRoutingService
            .Setup(x => x.GetContentRoute(It.IsAny<string>()))
            .Returns((string path) => path);
        var mockApiPublishedContentCache = new Mock<IApiPublishedContentCache>();
        mockApiPublishedContentCache
            .Setup(x => x.GetByRoute(TestPath))
            .Returns(new Mock<IPublishedContent>().Object);
        mockApiPublishedContentCache
            .Setup(x => x.GetByRoute("/"))
            .Returns(new Mock<IPublishedContent>().Object);
        return new ApiContentPathResolver(mockRequestRoutingService.Object, mockApiPublishedContentCache.Object);
    }
}
