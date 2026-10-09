using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides additional cache tags when caching a website content page.
/// </summary>
/// <remarks>
///     Multiple implementations can be registered; the output cache policy iterates all of them
///     to collect tags at cache-write time. Works as a pair with <see cref="IWebsiteOutputCacheEvictionProvider"/>:
///     the tag provider adds custom tags when caching, and the eviction provider maps content changes
///     back to those tags at eviction time.
/// </remarks>
public interface IWebsiteOutputCacheTagProvider
{
    /// <summary>
    ///     Returns additional cache tags for the given published content.
    /// </summary>
    /// <param name="content">The published content being cached.</param>
    /// <returns>Additional cache tags to associate with the cached response.</returns>
    IEnumerable<string> GetTags(IPublishedContent content);
}
