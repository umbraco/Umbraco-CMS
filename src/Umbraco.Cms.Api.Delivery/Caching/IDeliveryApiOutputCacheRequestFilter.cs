using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Determines whether a Delivery API request is eligible for output caching.
/// </summary>
/// <remarks>
///     <para>
///         This interface provides two levels of cacheability checks:
///     </para>
///     <list type="bullet">
///         <item><see cref="IsCacheable(HttpContext)"/> — called before the controller runs, for
///         request-level decisions (e.g. preview mode, access control).</item>
///         <item><see cref="IsCacheable(HttpContext, IPublishedContent)"/> — called after the controller
///         resolves content, for content-aware decisions (e.g. exclude specific content types).</item>
///     </list>
/// </remarks>
public interface IDeliveryApiOutputCacheRequestFilter
{
    /// <summary>
    ///     Gets a value indicating whether the request is eligible for output caching.
    ///     Called before the controller runs.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns><c>true</c> if the response may be cached; <c>false</c> to skip caching.</returns>
    bool IsCacheable(HttpContext context);

    /// <summary>
    ///     Gets a value indicating whether the response for the given content or media item is eligible
    ///     for output caching. Called after the controller resolves content.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="content">The resolved published content or media item.</param>
    /// <returns><c>true</c> if the response may be cached; <c>false</c> to skip caching.</returns>
    bool IsCacheable(HttpContext context, IPublishedContent content);
}
