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
    /// <typeparam name="TKey">The cache key type (string for documents, Guid for media).</typeparam>
    IConvertedPublishedContentCache<TKey> Create<TKey>(int? maximumItems, string cacheName)
        where TKey : notnull;
}
