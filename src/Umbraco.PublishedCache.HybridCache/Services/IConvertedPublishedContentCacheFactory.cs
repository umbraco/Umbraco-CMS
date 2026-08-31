using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Creates the L0 converted-content cache for a caching service, choosing the implementation based on the
///     configured maximum.
/// </summary>
internal interface IConvertedPublishedContentCacheFactory
{
    /// <summary>
    ///     Creates a cache instance.
    /// </summary>
    /// <param name="maximumItems">
    ///     The maximum number of entries to retain, or <c>null</c> for an unbounded cache.
    /// </param>
    /// <param name="cacheName">A human-readable name identifying the cache, used in startup logging.</param>
    /// <typeparam name="TKey">The cache key type (string for documents/elements, Guid for media).</typeparam>
    /// <typeparam name="TValue">The cached converted type (<see cref="IPublishedContent" /> or <see cref="IPublishedElement" />).</typeparam>
    IConvertedPublishedContentCache<TKey, TValue> Create<TKey, TValue>(int? maximumItems, string cacheName)
        where TKey : notnull
        where TValue : class, IPublishedElement;
}
