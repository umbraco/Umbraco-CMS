using BitFaster.Caching.Lfu;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Bounded;

/// <summary>
///     A bounded, scan-resistant L0 converted-content cache backed by BitFaster's W-TinyLFU
///     (<see cref="ConcurrentLfu{K,V}" />): frequently requested content is retained while rarely accessed
///     content is evicted, and a one-off full-tree walk cannot grow it without bound.
/// </summary>
/// <remarks>
///     The byte total is an <em>approximation</em> (the per-entry size is supplied by the caller), suitable for
///     diagnostics, not exact accounting.
/// </remarks>
/// <typeparam name="TKey">The cache key type (string for documents/elements, Guid for media).</typeparam>
/// <typeparam name="TValue">The cached converted type (<see cref="IPublishedContent" /> or <see cref="IPublishedElement" />).</typeparam>
internal sealed class BoundedConvertedPublishedContentCache<TKey, TValue> : IConvertedPublishedContentCache<TKey, TValue>
    where TKey : notnull
    where TValue : class, IPublishedElement
{
    // BitFaster's ConcurrentLfu partitions its capacity across admission window / probation / protected
    // regions and so requires a small minimum capacity.
    private const int MinimumCapacity = 3;

    // When estimating the byte size, size at most this many entries and scale by the count, so the estimate
    // stays cheap regardless of the configured capacity.
    private const int ByteEstimateSampleSize = 1000;

    private readonly ConcurrentLfu<TKey, CacheEntry> _cache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BoundedConvertedPublishedContentCache{TKey,TValue}" /> class.
    /// </summary>
    /// <param name="maximumItems">
    ///     The maximum number of entries to retain. Values below the W-TinyLFU minimum capacity are raised to it.
    /// </param>
    public BoundedConvertedPublishedContentCache(int maximumItems)
        => _cache = new ConcurrentLfu<TKey, CacheEntry>(Math.Max(MinimumCapacity, maximumItems));

    /// <inheritdoc />
    public long Count => _cache.Count;

    /// <inheritdoc />
    public long ApproximateSizeInBytes =>

        // Estimate by sizing a bounded sample and scaling by the entry count, so this stays cheap regardless
        // of the configured capacity. Enumerating is a snapshot read and does not register an access, so it
        // does not distort the eviction policy's frequency tracking.
        SampledSizeEstimator.Estimate(_cache.Count, _cache, entry => entry.Value.Size, ByteEstimateSampleSize);

    /// <inheritdoc />
    public bool TryGet(TKey key, out TValue? content)
    {
        if (_cache.TryGet(key, out CacheEntry entry))
        {
            content = entry.Content;
            return true;
        }

        content = null;
        return false;
    }

    /// <inheritdoc />
    public void Set(TKey key, TValue content, long approximateSizeInBytes)
        => _cache.AddOrUpdate(key, new CacheEntry(content, approximateSizeInBytes));

    /// <inheritdoc />
    public bool Remove(TKey key) => _cache.TryRemove(key);

    /// <inheritdoc />
    public void RemoveWhere(Func<TValue, bool> predicate)
    {
        // Enumerating is a snapshot read and does not register accesses against the eviction policy.
        foreach (KeyValuePair<TKey, CacheEntry> entry in _cache)
        {
            if (predicate(entry.Value.Content))
            {
                _cache.TryRemove(entry.Key);
            }
        }
    }

    /// <inheritdoc />
    public void Clear() => _cache.Clear();

    /// <summary>
    ///     Test support: synchronously drains the cache's read/write buffers and applies eviction, so size and
    ///     eviction are observable immediately.
    /// </summary>
    internal void RunPendingMaintenance() => _cache.DoMaintenance();

    private readonly record struct CacheEntry(TValue Content, long Size);
}
