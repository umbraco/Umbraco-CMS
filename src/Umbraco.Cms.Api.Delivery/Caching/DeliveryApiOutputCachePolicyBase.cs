using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Base output cache policy for Delivery API endpoints. Handles request filtering, vary-by rules,
///     and tagging. Subclasses specify the resolved-items key, tag prefix, and "all" tag that
///     distinguish content from media.
/// </summary>
internal abstract class DeliveryApiOutputCachePolicyBase : IOutputCachePolicy
{
    private readonly TimeSpan _defaultDuration;
    private readonly StringValues _defaultVaryByHeaders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiOutputCachePolicyBase"/> class.
    /// </summary>
    /// <param name="defaultDuration">The default cache duration from configuration.</param>
    /// <param name="defaultVaryByHeaders">The default vary-by headers for this endpoint type.</param>
    protected DeliveryApiOutputCachePolicyBase(TimeSpan defaultDuration, StringValues defaultVaryByHeaders)
    {
        _defaultDuration = defaultDuration;
        _defaultVaryByHeaders = defaultVaryByHeaders;
    }

    /// <summary>
    ///     Gets the <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/> key used to retrieve
    ///     resolved <see cref="IPublishedContent"/> items stashed by the controller.
    /// </summary>
    protected abstract string ResolvedItemsKey { get; }

    /// <summary>
    ///     Gets the tag prefix for individual item eviction (e.g. <c>umb-dapi-content-</c>).
    /// </summary>
    protected abstract string ItemTagPrefix { get; }

    /// <summary>
    ///     Gets the "all items" tag for bulk eviction (e.g. <c>umb-dapi-content-all</c>).
    /// </summary>
    protected abstract string AllItemsTag { get; }

    /// <summary>
    ///     Adds additional per-item tags to the output cache context. Called once per resolved item
    ///     during <c>ServeResponseAsync</c>. The default implementation does nothing.
    /// </summary>
    /// <param name="context">The output cache context.</param>
    /// <param name="item">The published content or media item.</param>
    /// <param name="services">The request service provider.</param>
    protected virtual void AddItemTags(OutputCacheContext context, IPublishedContent item, IServiceProvider services)
    {
    }

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        IServiceProvider services = context.HttpContext.RequestServices;
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        IDeliveryApiOutputCacheRequestFilter requestFilter = services.GetRequiredService<IDeliveryApiOutputCacheRequestFilter>();
        if (requestFilter.IsCacheable(context.HttpContext) is false)
        {
            context.EnableOutputCaching = false;
            logger.LogDebug("Request filter returned not cacheable — skipping output cache.");
            return ValueTask.CompletedTask;
        }

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = _defaultDuration;

        // Set default vary-by headers.
        context.CacheVaryByRules.HeaderNames = _defaultVaryByHeaders;

        // Invoke custom vary-by providers (additive, runs after defaults).
        IEnumerable<IDeliveryApiOutputCacheVaryByProvider> varyByProviders = services.GetServices<IDeliveryApiOutputCacheVaryByProvider>();
        foreach (IDeliveryApiOutputCacheVaryByProvider varyByProvider in varyByProviders)
        {
            varyByProvider.ConfigureVaryBy(context.HttpContext, context.CacheVaryByRules);
        }

        // Add base tags for bulk eviction.
        context.Tags.Add(AllItemsTag);
        context.Tags.Add(Constants.DeliveryApi.OutputCache.AllTag);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        if (context.HttpContext.Items[ResolvedItemsKey]
            is not IPublishedContent[] items || items.Length == 0)
        {
            return ValueTask.CompletedTask;
        }

        IServiceProvider services = context.HttpContext.RequestServices;
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
        IDeliveryApiOutputCacheRequestFilter requestFilter = services.GetRequiredService<IDeliveryApiOutputCacheRequestFilter>();
        IEnumerable<IDeliveryApiOutputCacheTagProvider> tagProviders = services.GetServices<IDeliveryApiOutputCacheTagProvider>();

        foreach (IPublishedContent item in items)
        {
            // Check content-aware cacheability.
            if (requestFilter.IsCacheable(context.HttpContext, item) is false)
            {
                context.AllowCacheStorage = false;
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Request filter returned not cacheable for item {ItemKey} — disabling cache storage.", item.Key);
                }

                return ValueTask.CompletedTask;
            }

            // Tag with specific item key for targeted eviction.
            context.Tags.Add(ItemTagPrefix + item.Key);

            // Allow subclasses to add additional per-item tags (e.g. ancestor tags for content).
            AddItemTags(context, item, services);

            // Invoke custom tag providers.
            foreach (IDeliveryApiOutputCacheTagProvider tagProvider in tagProviders)
            {
                foreach (var tag in tagProvider.GetTags(item))
                {
                    context.Tags.Add(tag);
                }
            }
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Caching Delivery API response with {TagCount} tags, duration {Duration}",
                context.Tags.Count,
                context.ResponseExpirationTimeSpan);
        }

        return ValueTask.CompletedTask;
    }
}
