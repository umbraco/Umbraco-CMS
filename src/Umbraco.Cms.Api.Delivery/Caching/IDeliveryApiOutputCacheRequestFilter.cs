using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Determines whether a Delivery API request is eligible for output caching.
/// </summary>
public interface IDeliveryApiOutputCacheRequestFilter
{
    /// <summary>
    ///     Returns <c>true</c> if the request is eligible for output caching; otherwise <c>false</c>.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns><c>true</c> if the response may be cached; <c>false</c> to skip caching.</returns>
    bool IsCacheable(HttpContext context);
}
