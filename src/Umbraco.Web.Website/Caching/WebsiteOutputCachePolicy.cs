using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Output cache policy for Umbraco website content pages. Controls when pages are cached,
///     how they are tagged for eviction, and how vary-by rules are configured.
/// </summary>
internal sealed class WebsiteOutputCachePolicy : IOutputCachePolicy
{
    private readonly TimeSpan _defaultDuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebsiteOutputCachePolicy"/> class.
    /// </summary>
    /// <param name="defaultDuration">The default cache duration from configuration.</param>
    public WebsiteOutputCachePolicy(TimeSpan defaultDuration)
        => _defaultDuration = defaultDuration;

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        IServiceProvider services = context.HttpContext.RequestServices;
        ILogger<WebsiteOutputCachePolicy> logger = services.GetRequiredService<ILogger<WebsiteOutputCachePolicy>>();

        UmbracoRouteValues? routeValues = context.HttpContext.Features.Get<UmbracoRouteValues>();
        if (routeValues is null)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Not an Umbraco content request — skipping output cache.");
            return ValueTask.CompletedTask;
        }

        IPublishedRequest publishedRequest = routeValues.PublishedRequest;

        if (publishedRequest.SetNoCacheHeader)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("SetNoCacheHeader is true — skipping output cache.");
            return ValueTask.CompletedTask;
        }

        IPublishedContent? content = publishedRequest.PublishedContent;
        if (content is null)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("No published content on request — skipping output cache.");
            return ValueTask.CompletedTask;
        }

        IWebsiteOutputCacheRequestFilter requestFilter = services.GetRequiredService<IWebsiteOutputCacheRequestFilter>();
        if (requestFilter.IsCacheable(context.HttpContext, content) is false)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Request filter returned not cacheable — skipping output cache.");
            return ValueTask.CompletedTask;
        }

        // Determine duration. A duration provider may return TimeSpan.Zero to disable caching
        // for specific content. A zero or negative resolved duration (from provider or config)
        // is treated as "do not cache".
        IWebsiteOutputCacheDurationProvider durationProvider = services.GetRequiredService<IWebsiteOutputCacheDurationProvider>();
        TimeSpan duration = durationProvider.GetDuration(content) ?? _defaultDuration;

        if (duration <= TimeSpan.Zero)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Resolved duration is zero or negative for content {ContentKey} — skipping output cache.", content.Key);
            return ValueTask.CompletedTask;
        }

        // Enable caching. AllowCacheLookup, AllowCacheStorage, and AllowLocking must be set
        // explicitly because they default to false on OutputCacheContext, and the ASP.NET Core
        // DefaultOutputCachePolicy does not reliably set them when a named policy is used.
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = duration;

        // Configure vary-by rules.
        IEnumerable<IWebsiteOutputCacheVaryByProvider> varyByProviders = services.GetServices<IWebsiteOutputCacheVaryByProvider>();
        foreach (IWebsiteOutputCacheVaryByProvider varyByProvider in varyByProviders)
        {
            varyByProvider.ConfigureVaryBy(context.HttpContext, context.CacheVaryByRules);
        }

        // Tag with content key.
        context.Tags.Add(Constants.Website.OutputCache.ContentTagPrefix + content.Key);
        context.Tags.Add(Constants.Website.OutputCache.AllContentTag);

        // Tag with ancestor keys for branch eviction.
        IDocumentNavigationQueryService navigationService = services.GetRequiredService<IDocumentNavigationQueryService>();
        if (navigationService.TryGetAncestorsKeys(content.Key, out IEnumerable<Guid> ancestorKeys))
        {
            foreach (Guid ancestorKey in ancestorKeys)
            {
                context.Tags.Add(Constants.Website.OutputCache.AncestorTagPrefix + ancestorKey);
            }
        }

        // Invoke custom tag providers.
        IEnumerable<IWebsiteOutputCacheTagProvider> tagProviders = services.GetServices<IWebsiteOutputCacheTagProvider>();
        foreach (IWebsiteOutputCacheTagProvider tagProvider in tagProviders)
        {
            foreach (var tag in tagProvider.GetTags(content))
            {
                context.Tags.Add(tag);
            }
        }

        logger.LogDebug(
            "Caching content {ContentKey} ({ContentTypeAlias}) for {Duration}, {TagCount} tags",
            content.Key,
            content.ContentType.Alias,
            duration,
            context.Tags.Count);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        ILogger<WebsiteOutputCachePolicy> logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<WebsiteOutputCachePolicy>>();

        // Don't cache responses that set cookies (e.g. antiforgery tokens on first request).
        if (!StringValues.IsNullOrEmpty(context.HttpContext.Response.Headers.SetCookie))
        {
            context.AllowCacheStorage = false;
            logger.LogDebug("Response has Set-Cookie header — preventing cache storage.");
        }

        // Don't cache responses marked as no-store (e.g. antiforgery middleware sets this
        // when @Html.AntiForgeryToken() is used in the view).
        StringValues cacheControl = context.HttpContext.Response.Headers.CacheControl;
        if (StringValues.IsNullOrEmpty(cacheControl) is false
            && cacheControl.ToString().Contains("no-store", StringComparison.OrdinalIgnoreCase))
        {
            context.AllowCacheStorage = false;
            logger.LogDebug("Response has Cache-Control: no-store — preventing cache storage.");
        }

        return ValueTask.CompletedTask;
    }
}
