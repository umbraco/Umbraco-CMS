namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Supplies a bounded, scan-resistant L0 converted-content cache. Provided by the opt-in bounded cache
///     package; absent from the default install. When no implementation is registered the L0 cache stays
///     unbounded regardless of the configured maximum.
/// </summary>
internal interface IBoundedConvertedPublishedContentCacheFactory
{
    /// <summary>
    ///     Creates a bounded cache that retains at most <paramref name="maximumItems" /> entries.
    /// </summary>
    /// <typeparam name="TKey">The cache key type (string for documents, Guid for media).</typeparam>
    IConvertedPublishedContentCache<TKey> CreateBounded<TKey>(int maximumItems)
        where TKey : notnull;
}
