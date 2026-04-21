using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Configures additional vary-by rules for Delivery API output caching.
/// </summary>
/// <remarks>
///     <para>
///         Multiple implementations can be registered; the output cache policy invokes all of them
///         to configure vary-by rules at cache-write time, after the default vary-by headers have been set.
///     </para>
///     <para>
///         Providers have direct access to <see cref="CacheVaryByRules"/> and can configure any aspect
///         including <see cref="CacheVaryByRules.QueryKeys"/>, <see cref="CacheVaryByRules.HeaderNames"/>,
///         and <see cref="CacheVaryByRules.VaryByValues"/>.
///     </para>
/// </remarks>
public interface IDeliveryApiOutputCacheVaryByProvider
{
    /// <summary>
    ///     Configures vary-by rules for the given request.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="rules">The vary-by rules to configure.</param>
    void ConfigureVaryBy(HttpContext context, CacheVaryByRules rules);
}
