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
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Output cache policy for Umbraco website content pages. Controls when pages are cached,
///     how they are tagged for eviction, and how vary-by rules are configured.
/// </summary>
internal sealed class WebsiteOutputCachePolicy : IOutputCachePolicy
{
    private readonly TimeSpan _defaultDuration;
    private readonly bool _enabled;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebsiteOutputCachePolicy"/> class.
    /// </summary>
    /// <param name="defaultDuration">The default cache duration from configuration.</param>
    /// <param name="enabled">Whether output caching is enabled.</param>
    public WebsiteOutputCachePolicy(TimeSpan defaultDuration, bool enabled)
    {
        _defaultDuration = defaultDuration;
        _enabled = enabled;
    }

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        IServiceProvider services = context.HttpContext.RequestServices;
        ILogger<WebsiteOutputCachePolicy> logger = services.GetRequiredService<ILogger<WebsiteOutputCachePolicy>>();

        if (_enabled is false)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Output caching disabled by configuration.");
            return ValueTask.CompletedTask;
        }

        UmbracoRouteValues? routeValues = context.HttpContext.Features.Get<UmbracoRouteValues>();
        if (routeValues is null)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Not an Umbraco content request — skipping output cache.");
            return ValueTask.CompletedTask;
        }

        IUmbracoContextAccessor umbracoContextAccessor = services.GetRequiredService<IUmbracoContextAccessor>();
        if (umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) && umbracoContext.InPreviewMode)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Preview mode active — skipping output cache.");
            return ValueTask.CompletedTask;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Authenticated member request — skipping output cache.");
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

        // Determine duration.
        IWebsiteOutputCacheDurationProvider durationProvider = services.GetRequiredService<IWebsiteOutputCacheDurationProvider>();
        TimeSpan? customDuration = durationProvider.GetDuration(content);
        if (customDuration == TimeSpan.Zero)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Duration provider returned zero for content {ContentKey} — skipping output cache.", content.Key);
            return ValueTask.CompletedTask;
        }

        TimeSpan duration = customDuration ?? _defaultDuration;

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
