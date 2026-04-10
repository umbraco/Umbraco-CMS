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
