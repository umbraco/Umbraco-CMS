using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Caching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Caching;

[TestFixture]
public class WebsiteOutputCachePolicyTests
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(30);

    private Mock<IUmbracoContextAccessor> _umbracoContextAccessorMock = null!;
    private Mock<IUmbracoContext> _umbracoContextMock = null!;
    private Mock<IWebsiteOutputCacheDurationProvider> _durationProviderMock = null!;
    private Mock<IDocumentNavigationQueryService> _navigationServiceMock = null!;

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

        _durationProviderMock = new Mock<IWebsiteOutputCacheDurationProvider>();
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns((TimeSpan?)null);

        _navigationServiceMock = new Mock<IDocumentNavigationQueryService>();
        IEnumerable<Guid> emptyKeys = Enumerable.Empty<Guid>();
        _navigationServiceMock
            .Setup(n => n.TryGetAncestorsKeys(It.IsAny<Guid>(), out emptyKeys))
            .Returns(true);
    }

    [Test]
    public async Task CacheRequestAsync_WhenDisabled_DoesNotCache()
    {
        var policy = CreatePolicy(enabled: false);
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.False);
    }

    [Test]
    public async Task CacheRequestAsync_WhenNoUmbracoRouteValues_DoesNotCache()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: false);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.False);
    }

    [Test]
    public async Task CacheRequestAsync_WhenPreviewMode_DoesNotCache()
    {
        _umbracoContextMock.Setup(c => c.InPreviewMode).Returns(true);
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.False);
    }

    [Test]
    public async Task CacheRequestAsync_WhenMemberAuthenticated_DoesNotCache()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true, isAuthenticated: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.False);
    }

    [Test]
    public async Task CacheRequestAsync_WhenSetNoCacheHeader_DoesNotCache()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true, setNoCacheHeader: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.False);
    }

    [Test]
    public async Task CacheRequestAsync_WhenDurationProviderReturnsZero_DoesNotCache()
    {
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns(TimeSpan.Zero);
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.False);
    }

    [Test]
    public async Task CacheRequestAsync_WhenEnabled_EnablesCaching()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.EnableOutputCaching, Is.True);
        Assert.That(context.AllowCacheLookup, Is.True);
        Assert.That(context.AllowCacheStorage, Is.True);
    }

    [Test]
    public async Task CacheRequestAsync_SetsDefaultDuration()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(DefaultDuration));
    }

    [Test]
    public async Task CacheRequestAsync_UsesDurationProvider_WhenNonNull()
    {
        var customDuration = TimeSpan.FromMinutes(5);
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns(customDuration);
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(customDuration));
    }

    [Test]
    public async Task CacheRequestAsync_FallsBackToConfigDuration_WhenProviderReturnsNull()
    {
        _durationProviderMock.Setup(p => p.GetDuration(It.IsAny<IPublishedContent>())).Returns((TimeSpan?)null);
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.ResponseExpirationTimeSpan, Is.EqualTo(DefaultDuration));
    }

    [Test]
    public async Task CacheRequestAsync_TagsWithContentKey()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Has.Some.StartsWith("umb-content-"));
    }

    [Test]
    public async Task CacheRequestAsync_TagsWithAllContentTag()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain("umb-content-all"));
    }

    [Test]
    public async Task CacheRequestAsync_TagsWithAncestorKeys()
    {
        var ancestorKey = Guid.NewGuid();
        IEnumerable<Guid> ancestorKeys = new[] { ancestorKey };
        _navigationServiceMock
            .Setup(n => n.TryGetAncestorsKeys(It.IsAny<Guid>(), out ancestorKeys))
            .Returns(true);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain($"umb-content-ancestor-{ancestorKey}"));
    }

    [Test]
    public async Task CacheRequestAsync_InvokesTagProviders()
    {
        var tagProviderMock = new Mock<IWebsiteOutputCacheTagProvider>();
        tagProviderMock.Setup(p => p.GetTags(It.IsAny<IPublishedContent>())).Returns(["custom-tag-1"]);

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true, tagProviders: [tagProviderMock.Object]);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.Tags, Does.Contain("custom-tag-1"));
    }

    [Test]
    public async Task CacheRequestAsync_InvokesVaryByProviders()
    {
        var varyByMock = new Mock<IWebsiteOutputCacheVaryByProvider>();
        varyByMock.Setup(p => p.ConfigureVaryBy(It.IsAny<HttpContext>(), It.IsAny<CacheVaryByRules>()))
            .Callback<HttpContext, CacheVaryByRules>((_, rules) => rules.VaryByValues["culture"] = "en");

        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true, varyByProviders: [varyByMock.Object]);

        await ((IOutputCachePolicy)policy).CacheRequestAsync(context, CancellationToken.None);

        Assert.That(context.CacheVaryByRules.VaryByValues["culture"], Is.EqualTo("en"));
    }

    [Test]
    public async Task ServeResponseAsync_WhenSetCookieHeader_DisallowsCacheStorage()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);
        context.HttpContext.Response.Headers.SetCookie = "test=value";

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.AllowCacheStorage, Is.False);
    }

    [Test]
    public async Task ServeResponseAsync_WhenCacheControlNoStore_DisallowsCacheStorage()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);
        context.HttpContext.Response.Headers.CacheControl = "no-cache, no-store";

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.AllowCacheStorage, Is.False);
    }

    [Test]
    public async Task ServeResponseAsync_WhenNoSetCookieHeader_DoesNotChangeCacheStorage()
    {
        var policy = CreatePolicy();
        OutputCacheContext context = CreateOutputCacheContext(withRouteValues: true);
        var originalValue = context.AllowCacheStorage;

        await ((IOutputCachePolicy)policy).ServeResponseAsync(context, CancellationToken.None);

        Assert.That(context.AllowCacheStorage, Is.EqualTo(originalValue));
    }

    private static WebsiteOutputCachePolicy CreatePolicy(bool enabled = true)
        => new(DefaultDuration, enabled);

    private OutputCacheContext CreateOutputCacheContext(
        bool withRouteValues = false,
        bool isAuthenticated = false,
        bool setNoCacheHeader = false,
        IEnumerable<IWebsiteOutputCacheTagProvider>? tagProviders = null,
        IEnumerable<IWebsiteOutputCacheVaryByProvider>? varyByProviders = null)
    {
        var contentKey = Guid.NewGuid();
        var contentType = Mock.Of<IPublishedContentType>(ct => ct.Alias == "testPage");
        var content = Mock.Of<IPublishedContent>(c =>
            c.Key == contentKey &&
            c.ContentType == contentType);

        var services = new ServiceCollection();
        services.AddSingleton<ILogger<WebsiteOutputCachePolicy>>(NullLogger<WebsiteOutputCachePolicy>.Instance);
        services.AddSingleton(_umbracoContextAccessorMock.Object);
        services.AddSingleton(_durationProviderMock.Object);
        services.AddSingleton(_navigationServiceMock.Object);
        services.AddSingleton<IEnumerable<IWebsiteOutputCacheTagProvider>>(
            (tagProviders ?? new IWebsiteOutputCacheTagProvider[] { new DefaultWebsiteOutputCacheTagProvider() }).ToList());
        services.AddSingleton<IEnumerable<IWebsiteOutputCacheVaryByProvider>>(
            (varyByProviders ?? Array.Empty<IWebsiteOutputCacheVaryByProvider>()).ToList());

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        if (isAuthenticated)
        {
            var identity = new System.Security.Claims.ClaimsIdentity("TestAuth");
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }

        if (withRouteValues)
        {
            var publishedRequest = Mock.Of<IPublishedRequest>(r =>
                r.PublishedContent == content &&
                r.SetNoCacheHeader == setNoCacheHeader);

            var controllerDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor
            {
                ControllerName = "RenderController",
                ActionName = "Index",
                ControllerTypeInfo = typeof(global::Umbraco.Cms.Web.Common.Controllers.RenderController).GetTypeInfo(),
            };

            var routeValues = new UmbracoRouteValues(publishedRequest, controllerDescriptor);
            httpContext.Features.Set(routeValues);
        }

        return new OutputCacheContext { HttpContext = httpContext };
    }
}
