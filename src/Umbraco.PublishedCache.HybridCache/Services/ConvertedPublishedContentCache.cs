using System.Collections.Concurrent;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Encapsulates the in-process (L0) cache of converted <see cref="IPublishedElement" /> behind a single
///     insert/remove/clear path, tracking both the entry count and an approximate retained byte total.
/// </summary>
/// <remarks>
///     Routing every mutation through this one type keeps the byte total consistent by construction —
///     there is exactly one place that adds on insert and subtracts on remove/clear. The byte total is an
///     <em>approximation</em> (the per-entry size is supplied by the caller and the running total is updated
///     without locking the whole structure), suitable for diagnostics, not exact accounting.
///     This is also the seam the later bounded/eviction-aware implementation slots into.
/// </remarks>
/// <typeparam name="TKey">The cache key type.</typeparam>
/// <typeparam name="TValue">The cached converted type.</typeparam>
internal sealed class ConvertedPublishedContentCache<TKey, TValue>
    where TKey : notnull
    where TValue : class, IPublishedElement
{
    private readonly ConcurrentDictionary<TKey, CacheEntry> _cache = new();
    private long _approximateSizeInBytes;

    /// <summary>
    ///     Gets the number of entries currently held.
    /// </summary>
    public long Count => _cache.Count;

    /// <summary>
    ///     Gets the approximate retained size, in bytes, of the cached entries.
    /// </summary>
    public long ApproximateSizeInBytes => Interlocked.Read(ref _approximateSizeInBytes);

    /// <summary>
    ///     Attempts to get a cached converted content item.
    /// </summary>
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

    /// <summary>
    ///     Adds or replaces a cached converted content item, adjusting the running byte total by the supplied
    ///     per-entry size estimate.
    /// </summary>
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

    /// <summary>
    ///     Removes a cached entry, subtracting its size from the running total.
    /// </summary>
    public bool Remove(TKey key)
    {
        if (_cache.TryRemove(key, out CacheEntry entry))
        {
            Interlocked.Add(ref _approximateSizeInBytes, -entry.Size);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Removes every entry whose content matches the predicate, subtracting their sizes from the total.
    /// </summary>
    public void RemoveWhere(Func<TValue, bool> predicate)
    {
        foreach (KeyValuePair<TKey, CacheEntry> kvp in _cache)
        {
            if (predicate(kvp.Value.Content) && _cache.TryRemove(kvp.Key, out CacheEntry removed))
            {
                Interlocked.Add(ref _approximateSizeInBytes, -removed.Size);
            }
        }
    }

    /// <summary>
    ///     Removes all entries and resets the running byte total.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        Interlocked.Exchange(ref _approximateSizeInBytes, 0);
    }

    private readonly record struct CacheEntry(TValue Content, long Size);
}
