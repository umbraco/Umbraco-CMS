// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Testing;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.UmbracoContext;

[TestFixture]
public class UmbracoContextTests
{
    [Test]
    public void OriginalRequestUrl_Without_HttpContext_Uses_ApplicationMainUrl()
    {
        // Arrange - simulate a background service scenario: no HttpContext, but ApplicationMainUrl is configured
        var umbracoContext = CreateUmbracoContext(
            applicationMainUrl: new Uri("https://example.com"),
            httpContext: null);

        // Act
        var originalRequestUrl = umbracoContext.OriginalRequestUrl;

        // Assert - should use the configured ApplicationMainUrl with https scheme, not the hardcoded http://localhost
        Assert.AreEqual("https", originalRequestUrl.Scheme);
        Assert.AreEqual("example.com", originalRequestUrl.Host);
    }

    [Test]
    public void OriginalRequestUrl_Without_HttpContext_Without_ApplicationMainUrl_Falls_Back_To_Localhost()
    {
        // Arrange - no HttpContext and no ApplicationMainUrl configured
        var umbracoContext = CreateUmbracoContext(
            applicationMainUrl: null,
            httpContext: null);

        // Act
        var originalRequestUrl = umbracoContext.OriginalRequestUrl;

        // Assert - should fall back to http://localhost when no ApplicationMainUrl is available
        Assert.AreEqual("http", originalRequestUrl.Scheme);
        Assert.AreEqual("localhost", originalRequestUrl.Host);
    }

    [Test]
    public void OriginalRequestUrl_With_HttpContext_Uses_Request_Url()
    {
        // Arrange - normal HTTP request scenario
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("my-site.com");
        httpContext.Request.Path = "/some-page";

        var umbracoContext = CreateUmbracoContext(
            applicationMainUrl: new Uri("https://example.com"),
            httpContext: httpContext);

        // Act
        var originalRequestUrl = umbracoContext.OriginalRequestUrl;

        // Assert - should use the actual request URL, not ApplicationMainUrl
        Assert.AreEqual("https", originalRequestUrl.Scheme);
        Assert.AreEqual("my-site.com", originalRequestUrl.Host);
    }

    [Test]
    public void OriginalRequestUrl_Fallback_Is_Not_Cached_And_Picks_Up_Later_ApplicationMainUrl()
    {
        // Arrange - simulate early startup: no HttpContext, no ApplicationMainUrl yet.
        // Then ApplicationMainUrl becomes available (auto-detected from first HTTP request).
        var hostingEnvironmentMock = new Mock<IHostingEnvironment>();
        hostingEnvironmentMock.Setup(x => x.ApplicationVirtualPath).Returns("/");
        hostingEnvironmentMock.Setup(x => x.ToAbsolute(It.IsAny<string>()))
            .Returns((string path) => path.TrimStart('~'));
        hostingEnvironmentMock.Setup(x => x.ApplicationMainUrl).Returns((Uri?)null);

        var umbracoContext = CreateUmbracoContextWithHostingEnvironment(
            hostingEnvironmentMock.Object,
            httpContext: null);

        // Act - first access falls back to http://localhost
        var firstUrl = umbracoContext.OriginalRequestUrl;
        Assert.AreEqual("http", firstUrl.Scheme);
        Assert.AreEqual("localhost", firstUrl.Host);

        // Simulate ApplicationMainUrl becoming available (e.g. after first HTTP request)
        hostingEnvironmentMock.Setup(x => x.ApplicationMainUrl).Returns(new Uri("https://example.com"));

        // Act - second access should pick up the newly available ApplicationMainUrl
        var secondUrl = umbracoContext.OriginalRequestUrl;
        Assert.AreEqual("https", secondUrl.Scheme);
        Assert.AreEqual("example.com", secondUrl.Host);
    }

    private static global::Umbraco.Cms.Web.Common.UmbracoContext.UmbracoContext CreateUmbracoContext(
        Uri? applicationMainUrl,
        HttpContext? httpContext)
    {
        var webRoutingSettings = new WebRoutingSettings();
        if (applicationMainUrl is not null)
        {
            webRoutingSettings.UmbracoApplicationUrl = applicationMainUrl.ToString();
        }

        var hostingEnvironment = new TestHostingEnvironment(
            Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == new HostingSettings()),
            Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == webRoutingSettings),
            Mock.Of<IWebHostEnvironment>(x =>
                x.WebRootPath == "/" &&
                x.ContentRootPath == "/"));

        return CreateUmbracoContextWithHostingEnvironment(hostingEnvironment, httpContext);
    }

    private static global::Umbraco.Cms.Web.Common.UmbracoContext.UmbracoContext CreateUmbracoContextWithHostingEnvironment(
        IHostingEnvironment hostingEnvironment,
        HttpContext? httpContext)
    {
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        var umbracoRequestPaths = new UmbracoRequestPaths(
            hostingEnvironment,
            Options.Create(new UmbracoRequestPathsOptions()));
        var uriUtility = new UriUtility(hostingEnvironment);
        var cacheManager = Mock.Of<ICacheManager>(x =>
            x.Content == Mock.Of<IPublishedContentCache>() &&
            x.Media == Mock.Of<IPublishedMediaCache>());

        return new global::Umbraco.Cms.Web.Common.UmbracoContext.UmbracoContext(
            umbracoRequestPaths,
            hostingEnvironment,
            uriUtility,
            Mock.Of<ICookieManager>(),
            httpContextAccessor,
            Mock.Of<IWebProfilerService>(),
            cacheManager);
    }
}
