using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Caching;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Caching;

[TestFixture]
public class DefaultDeliveryApiOutputCacheRequestFilterTests
{
    private Mock<IRequestPreviewService> _previewServiceMock = null!;
    private Mock<IApiAccessService> _accessServiceMock = null!;
    private DefaultDeliveryApiOutputCacheRequestFilter _filter = null!;

    [SetUp]
    public void SetUp()
    {
        _previewServiceMock = new Mock<IRequestPreviewService>();
        _previewServiceMock.Setup(p => p.IsPreview()).Returns(false);

        _accessServiceMock = new Mock<IApiAccessService>();
        _accessServiceMock.Setup(a => a.HasPublicAccess()).Returns(true);

        _filter = new DefaultDeliveryApiOutputCacheRequestFilter(_previewServiceMock.Object, _accessServiceMock.Object);
    }

    [Test]
    public void IsCacheable_WhenNotPreviewAndPublicAccess_ReturnsTrue()
    {
        var httpContext = new DefaultHttpContext();

        Assert.That(_filter.IsCacheable(httpContext), Is.True);
    }

    [Test]
    public void IsCacheable_WhenPreview_ReturnsFalse()
    {
        _previewServiceMock.Setup(p => p.IsPreview()).Returns(true);
        var httpContext = new DefaultHttpContext();

        Assert.That(_filter.IsCacheable(httpContext), Is.False);
    }

    [Test]
    public void IsCacheable_WhenNoPublicAccess_ReturnsFalse()
    {
        _accessServiceMock.Setup(a => a.HasPublicAccess()).Returns(false);
        var httpContext = new DefaultHttpContext();

        Assert.That(_filter.IsCacheable(httpContext), Is.False);
    }

    [Test]
    public void IsCacheable_WithContent_ReturnsTrue()
    {
        var httpContext = new DefaultHttpContext();
        var content = Mock.Of<IPublishedContent>();

        Assert.That(_filter.IsCacheable(httpContext, content), Is.True);
    }
}
