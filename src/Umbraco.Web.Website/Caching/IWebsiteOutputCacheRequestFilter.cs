using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Determines whether a request is eligible for output caching.
/// </summary>
public interface IWebsiteOutputCacheRequestFilter
{
    /// <summary>
    ///     Returns <c>true</c> if the request is eligible for output caching; otherwise <c>false</c>.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="content">The published content being rendered.</param>
    /// <returns><c>true</c> if the response may be cached; <c>false</c> to skip caching.</returns>
    bool IsCacheable(HttpContext context, IPublishedContent content);
}
