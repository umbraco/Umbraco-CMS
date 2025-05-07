using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Services;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Services;

[TestFixture]
public class RequestRoutingServiceTests
{
    [TestCase(null)]
    [TestCase("")]
    public void Empty_Path_Yields_Nothing(string? path)
    {
        var subject = new RequestRoutingService(
            Mock.Of<IDomainCache>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IRequestStartItemProviderAccessor>(),
            Mock.Of<IRequestCultureService>(),
            Mock.Of<IVariationContextAccessor>());

        var result = subject.GetContentRoute(path!);
        Assert.IsEmpty(result);
    }

    [Test]
    public void Explicit_Start_Item_Yields_Path_Prefixed_With_Start_Item_Id()
    {
        var startItem = new Mock<IPublishedContent>();
        startItem.SetupGet(m => m.Id).Returns(1234);

        var requestStartItemProviderMock = new Mock<IRequestStartItemProvider>();
        requestStartItemProviderMock.Setup(m => m.GetStartItem()).Returns(startItem.Object);

        var requestStartItemProvider = requestStartItemProviderMock.Object;
        var requestStartItemProviderAccessorMock = new Mock<IRequestStartItemProviderAccessor>();
        requestStartItemProviderAccessorMock.Setup(m => m.TryGetValue(out requestStartItemProvider)).Returns(true);

        var subject = new RequestRoutingService(
            Mock.Of<IDomainCache>(),
            Mock.Of<IHttpContextAccessor>(),
            requestStartItemProviderAccessorMock.Object,
            Mock.Of<IRequestCultureService>(),
            Mock.Of<IVariationContextAccessor>());

        var result = subject.GetContentRoute("/some/where");
        Assert.AreEqual("1234/some/where", result);
    }

    [TestCase("some/where")]
    [TestCase("/some/where")]
    public void Without_Domain_Binding_Yields_Path_Prefixed_With_Slash(string requestedPath)
    {
        var requestStartItemProviderMock = new Mock<IRequestStartItemProvider>();
        var requestStartItemProvider = requestStartItemProviderMock.Object;
        var requestStartItemProviderAccessorMock = new Mock<IRequestStartItemProviderAccessor>();
        requestStartItemProviderAccessorMock.Setup(m => m.TryGetValue(out requestStartItemProvider)).Returns(true);

        var httpContextAccessorMock = CreateHttpContextAccessorMock();

        var subject = new RequestRoutingService(
            Mock.Of<IDomainCache>(),
            httpContextAccessorMock.Object,
            requestStartItemProviderAccessorMock.Object,
            Mock.Of<IRequestCultureService>(),
            Mock.Of<IVariationContextAccessor>());

        var result = subject.GetContentRoute(requestedPath);
        Assert.AreEqual("/some/where", result);
    }

    [TestCase("some/where")]
    [TestCase("/some/where")]
    public void With_Domain_Binding_Yields_Path_Prefixed_With_Domain_Content_Id(string requestedPath)
    {
        var requestStartItemProviderMock = new Mock<IRequestStartItemProvider>();
        var requestStartItemProvider = requestStartItemProviderMock.Object;
        var requestStartItemProviderAccessorMock = new Mock<IRequestStartItemProviderAccessor>();
        requestStartItemProviderAccessorMock.Setup(m => m.TryGetValue(out requestStartItemProvider)).Returns(true);

        var httpContextAccessorMock = CreateHttpContextAccessorMock();

        var domainCacheMock = GetDomainCacheMock(null);

        var subject = new RequestRoutingService(
            domainCacheMock.Object,
            httpContextAccessorMock.Object,
            requestStartItemProviderAccessorMock.Object,
            Mock.Of<IRequestCultureService>(),
            Mock.Of<IVariationContextAccessor>());

        var result = subject.GetContentRoute(requestedPath);
        Assert.AreEqual("1234/some/where", result);
    }

    [Test]
    public void Domain_Binding_Culture_Sets_Variation_Context()
    {
        var requestStartItemProviderMock = new Mock<IRequestStartItemProvider>();
        var requestStartItemProvider = requestStartItemProviderMock.Object;
        var requestStartItemProviderAccessorMock = new Mock<IRequestStartItemProviderAccessor>();
        requestStartItemProviderAccessorMock.Setup(m => m.TryGetValue(out requestStartItemProvider)).Returns(true);

        var httpContextAccessorMock = CreateHttpContextAccessorMock();

        var domainCacheMock = GetDomainCacheMock("da-DK");

        var variationContextAccessor = new TestVariationContextAccessor();

        var subject = new RequestRoutingService(
            domainCacheMock.Object,
            httpContextAccessorMock.Object,
            requestStartItemProviderAccessorMock.Object,
            Mock.Of<IRequestCultureService>(),
            variationContextAccessor);

        var result = subject.GetContentRoute("/some/where");
        Assert.AreEqual("1234/some/where", result);
        Assert.IsNotNull(variationContextAccessor.VariationContext);
        Assert.AreEqual("da-DK", variationContextAccessor.VariationContext.Culture);
    }

    [Test]
    public void Domain_Binding_Culture_Does_Not_Overwrite_Existing_Segment_Context()
    {
        var requestStartItemProviderMock = new Mock<IRequestStartItemProvider>();
        var requestStartItemProvider = requestStartItemProviderMock.Object;
        var requestStartItemProviderAccessorMock = new Mock<IRequestStartItemProviderAccessor>();
        requestStartItemProviderAccessorMock.Setup(m => m.TryGetValue(out requestStartItemProvider)).Returns(true);

        var httpContextAccessorMock = CreateHttpContextAccessorMock();

        var domainCacheMock = GetDomainCacheMock("da-DK");

        var variationContextAccessor = new TestVariationContextAccessor
        {
            VariationContext = new VariationContext("it-IT", "some-segment")
        };

        var subject = new RequestRoutingService(
            domainCacheMock.Object,
            httpContextAccessorMock.Object,
            requestStartItemProviderAccessorMock.Object,
            Mock.Of<IRequestCultureService>(),
            variationContextAccessor);

        var result = subject.GetContentRoute("/some/where");
        Assert.AreEqual("1234/some/where", result);
        Assert.IsNotNull(variationContextAccessor.VariationContext);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("da-DK", variationContextAccessor.VariationContext.Culture);
            Assert.AreEqual("some-segment", variationContextAccessor.VariationContext.Segment);
        });
    }

    [Test]
    public void Explicit_Request_Culture_Overrides_Domain_Binding_Culture()
    {
        var requestStartItemProviderMock = new Mock<IRequestStartItemProvider>();
        var requestStartItemProvider = requestStartItemProviderMock.Object;
        var requestStartItemProviderAccessorMock = new Mock<IRequestStartItemProviderAccessor>();
        requestStartItemProviderAccessorMock.Setup(m => m.TryGetValue(out requestStartItemProvider)).Returns(true);

        var httpContextAccessorMock = CreateHttpContextAccessorMock();

        var domainCacheMock = GetDomainCacheMock("da-DK");

        const string expectedCulture = "it-IT";
        const string expectedSegment = "some-segment";
        var variationContextAccessor = new TestVariationContextAccessor
        {
            VariationContext = new VariationContext(expectedCulture, expectedSegment)
        };

        var requestCultureServiceMock = new Mock<IRequestCultureService>();
        requestCultureServiceMock
            .Setup(m => m.GetRequestedCulture())
            .Returns(expectedCulture);

        var subject = new RequestRoutingService(
            domainCacheMock.Object,
            httpContextAccessorMock.Object,
            requestStartItemProviderAccessorMock.Object,
            requestCultureServiceMock.Object,
            variationContextAccessor);

        var result = subject.GetContentRoute("/some/where");
        Assert.AreEqual("1234/some/where", result);
        Assert.IsNotNull(variationContextAccessor.VariationContext);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(expectedCulture, variationContextAccessor.VariationContext.Culture);
            Assert.AreEqual(expectedSegment, variationContextAccessor.VariationContext.Segment);
        });
    }

    private static Mock<IHttpContextAccessor> CreateHttpContextAccessorMock()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .SetupGet(m => m.HttpContext)
            .Returns(
                new DefaultHttpContext
                {
                    Request =
                    {
                        Scheme = "https",
                        Host = new HostString("some.host"),
                        Path = "/",
                        QueryString = new QueryString(string.Empty)
                    }
                });
        return httpContextAccessorMock;
    }

    private static Mock<IDomainCache> GetDomainCacheMock(string? culture)
    {
        var domainCacheMock = new Mock<IDomainCache>();
        domainCacheMock
            .Setup(m => m.GetAll(It.IsAny<bool>()))
            .Returns([
                new Domain(1, "some.host", 1234, culture, false, 1)
            ]);
        return domainCacheMock;
    }
}
