using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     The unbounded (historical) L0 converted-content cache: a plain <see cref="ConcurrentDictionary{TKey,TValue}" />
///     that only ever evicts on content change or explicit clear. Walking the whole published tree (Delivery API
///     crawl, sitemap, warm-up) therefore retains the whole tree's converted form.
/// </summary>
/// <typeparam name="TKey">The cache key type.</typeparam>
/// <typeparam name="TValue">The cached converted type.</typeparam>
internal sealed class UnboundedConvertedPublishedContentCache<TKey, TValue> : IConvertedPublishedContentCache<TKey, TValue>
    where TKey : notnull
    where TValue : class, IPublishedElement
{
    private readonly ConcurrentDictionary<TKey, CacheEntry> _cache = new();
    private long _approximateSizeInBytes;

    /// <inheritdoc />
    public long Count => _cache.Count;

    /// <inheritdoc />
    public long ApproximateSizeInBytes => Math.Max(0, Interlocked.Read(ref _approximateSizeInBytes));

    /// <inheritdoc />
    public bool TryGet(TKey key, out TValue? content)
    {
        if (_cache.TryGetValue(key, out CacheEntry entry))
        {
            content = entry.Content;
            return true;
        }

        content = null;
        return false;
    }

    /// <inheritdoc />
    public void Set(TKey key, TValue content, long approximateSizeInBytes)
    {
        var entry = new CacheEntry(content, approximateSizeInBytes);

        // Compute the delta against any existing entry so overwrites don't inflate the total. A concurrent
        // Set/Remove for the same key can make this off by one entry's size; acceptable for a diagnostic
        // counter, and Clear() re-establishes the baseline.
        long delta = approximateSizeInBytes;
        if (_cache.TryGetValue(key, out CacheEntry existing))
        {
            delta -= existing.Size;
        }

        _cache[key] = entry;
        Interlocked.Add(ref _approximateSizeInBytes, delta);
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if (_cache.TryRemove(key, out CacheEntry entry))
        {
            Interlocked.Add(ref _approximateSizeInBytes, -entry.Size);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void RemoveWhere(Func<TValue, bool> predicate)
    {
        foreach (KeyValuePair<TKey, CacheEntry> entry in _cache)
        {
            if (predicate(entry.Value.Content) && _cache.TryRemove(entry.Key, out CacheEntry removed))
            {
                Interlocked.Add(ref _approximateSizeInBytes, -removed.Size);
            }
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _approximateSizeInBytes, 0);
    }

    private readonly record struct CacheEntry(TValue Content, long Size);
}
