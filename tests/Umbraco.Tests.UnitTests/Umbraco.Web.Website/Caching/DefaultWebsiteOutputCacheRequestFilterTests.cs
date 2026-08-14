using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Website.Caching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Caching;

[TestFixture]
public class DefaultWebsiteOutputCacheRequestFilterTests
{
    private Mock<IUmbracoContextAccessor> _umbracoContextAccessorMock = null!;
    private Mock<IUmbracoContext> _umbracoContextMock = null!;
    private DefaultWebsiteOutputCacheRequestFilter _filter = null!;

    [SetUp]
    public void SetUp()
    {
        _umbracoContextMock = new Mock<IUmbracoContext>();
        _umbracoContextMock.Setup(c => c.InPreviewMode).Returns(false);

        _umbracoContextAccessorMock = new Mock<IUmbracoContextAccessor>();
        IUmbracoContext? outContext = _umbracoContextMock.Object;
        _umbracoContextAccessorMock
            .Setup(a => a.TryGetUmbracoContext(out outContext))
            .Returns(true);

        _filter = new DefaultWebsiteOutputCacheRequestFilter(_umbracoContextAccessorMock.Object);
    }

    [Test]
    public void IsCacheable_WhenAnonymousAndNotPreview_ReturnsTrue()
    {
        var httpContext = new DefaultHttpContext();
        var content = Mock.Of<IPublishedContent>();

        Assert.That(_filter.IsCacheable(httpContext, content), Is.True);
    }

    [Test]
    public void IsCacheable_WhenPreviewMode_ReturnsFalse()
    {
        _umbracoContextMock.Setup(c => c.InPreviewMode).Returns(true);
        var httpContext = new DefaultHttpContext();
        var content = Mock.Of<IPublishedContent>();

        Assert.That(_filter.IsCacheable(httpContext, content), Is.False);
    }

    [Test]
    public void IsCacheable_WhenAuthenticated_ReturnsFalse()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity("TestAuth"))
        };
        var content = Mock.Of<IPublishedContent>();

        Assert.That(_filter.IsCacheable(httpContext, content), Is.False);
    }

    [Test]
    public void IsCacheable_WhenNoUmbracoContext_ReturnsTrue()
    {
        IUmbracoContext? nullContext = null;
        _umbracoContextAccessorMock
            .Setup(a => a.TryGetUmbracoContext(out nullContext))
            .Returns(false);

        var httpContext = new DefaultHttpContext();
        var content = Mock.Of<IPublishedContent>();

        Assert.That(_filter.IsCacheable(httpContext, content), Is.True);
    }
}
