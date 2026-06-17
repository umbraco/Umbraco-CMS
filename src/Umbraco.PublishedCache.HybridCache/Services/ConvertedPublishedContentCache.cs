using System.Collections.Concurrent;
using BitFaster.Caching.Lfu;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     Encapsulates the in-process (L0) cache of converted <see cref="IPublishedContent" /> behind a single
///     insert/remove/clear path, tracking both the entry count and an approximate retained byte total.
/// </summary>
/// <remarks>
///     <para>
///         When constructed without a maximum it is unbounded — a plain <see cref="ConcurrentDictionary{TKey,TValue}" />
///         that only ever evicts on content change or explicit clear (the historical behaviour). When given a
///         maximum it becomes a bounded, scan-resistant cache backed by BitFaster's W-TinyLFU
///         (<see cref="ConcurrentLfu{K,V}" />): frequently requested content is retained while rarely accessed
///         content is evicted, and a one-off full-tree walk cannot grow it without bound.
///     </para>
///     <para>
///         The byte total is an <em>approximation</em> (the per-entry size is supplied by the caller), suitable
///         for diagnostics, not exact accounting.
///     </para>
/// </remarks>
/// <typeparam name="TKey">The cache key type (string for documents, Guid for media).</typeparam>
internal sealed class ConvertedPublishedContentCache<TKey>
    where TKey : notnull
{
    // BitFaster's ConcurrentLfu partitions its capacity across admission window / probation / protected
    // regions and so requires a small minimum capacity.
    private const int MinimumBoundedCapacity = 3;

    // When estimating the byte size of the bounded cache, size at most this many entries and scale by the
    // count, so the estimate stays cheap regardless of the configured capacity.
    private const int ByteEstimateSampleSize = 1000;

    private readonly ConcurrentDictionary<TKey, CacheEntry>? _unbounded;
    private readonly ConcurrentLfu<TKey, CacheEntry>? _bounded;
    private long _approximateSizeInBytes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConvertedPublishedContentCache{TKey}" /> class.
    /// </summary>
    /// <param name="maximumItems">
    ///     The maximum number of entries to retain, or <c>null</c> for an unbounded cache.
    /// </param>
    public ConvertedPublishedContentCache(int? maximumItems = null)
    {
        if (maximumItems is null)
        {
            _unbounded = new ConcurrentDictionary<TKey, CacheEntry>();
        }
        else
        {
            _bounded = new ConcurrentLfu<TKey, CacheEntry>(Math.Max(MinimumBoundedCapacity, maximumItems.Value));
        }
    }

    /// <summary>
    ///     Gets the number of entries currently held.
    /// </summary>
    public long Count => _unbounded?.Count ?? _bounded!.Count;

    /// <summary>
    ///     Gets the approximate retained size, in bytes, of the cached entries.
    /// </summary>
    public long ApproximateSizeInBytes
    {
        get
        {
            if (_unbounded is not null)
            {
                return Math.Max(0, Interlocked.Read(ref _approximateSizeInBytes));
            }

            // Bounded: estimate by sizing a bounded sample and scaling by the entry count, so this stays
            // cheap regardless of the configured capacity. Enumerating is a snapshot read and does not
            // register an access, so it does not distort the eviction policy's frequency tracking.
            return SampledSizeEstimator.Estimate(_bounded!.Count, _bounded, entry => entry.Value.Size, ByteEstimateSampleSize);
        }
    }

    /// <summary>
    ///     Attempts to get a cached converted content item.
    /// </summary>
    public bool TryGet(TKey key, out IPublishedContent? content)
    {
        bool found = _unbounded is not null
            ? _unbounded.TryGetValue(key, out CacheEntry entry)
            : _bounded!.TryGet(key, out entry);

        if (found)
        {
            content = entry.Content;
            return true;
        }

        content = null;
        return false;
    }

    /// <summary>
    ///     Adds or replaces a cached converted content item.
    /// </summary>
    public void Set(TKey key, IPublishedContent content, long approximateSizeInBytes)
    {
        var entry = new CacheEntry(content, approximateSizeInBytes);

        if (_unbounded is null)
        {
            _bounded!.AddOrUpdate(key, entry);
            return;
        }

        // Compute the delta against any existing entry so overwrites don't inflate the total. A concurrent
        // Set/Remove for the same key can make this off by one entry's size; acceptable for a diagnostic
        // counter, and Clear() re-establishes the baseline.
        long delta = approximateSizeInBytes;
        if (_unbounded.TryGetValue(key, out CacheEntry existing))
        {
            delta -= existing.Size;
        }

        _unbounded[key] = entry;
        Interlocked.Add(ref _approximateSizeInBytes, delta);
    }

    /// <summary>
    ///     Removes a cached entry.
    /// </summary>
    public bool Remove(TKey key)
    {
        if (_unbounded is null)
        {
            return _bounded!.TryRemove(key);
        }

        if (_unbounded.TryRemove(key, out CacheEntry entry))
        {
            Interlocked.Add(ref _approximateSizeInBytes, -entry.Size);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Removes every entry whose content matches the predicate.
    /// </summary>
    public void RemoveWhere(Func<IPublishedContent, bool> predicate)
    {
        if (_unbounded is null)
        {
            // Enumerating is a snapshot read and does not register accesses against the eviction policy.
            foreach (KeyValuePair<TKey, CacheEntry> entry in _bounded!)
            {
                if (predicate(entry.Value.Content))
                {
                    _bounded.TryRemove(entry.Key);
                }
            }

            return;
        }

        foreach (KeyValuePair<TKey, CacheEntry> entry in _unbounded)
        {
            if (predicate(entry.Value.Content) && _unbounded.TryRemove(entry.Key, out CacheEntry removed))
            {
                Interlocked.Add(ref _approximateSizeInBytes, -removed.Size);
            }
        }
    }

    /// <summary>
    ///     Removes all entries.
    /// </summary>
    public void Clear()
    {
        if (_unbounded is null)
        {
            _bounded!.Clear();
            return;
        }

        _unbounded.Clear();
        Interlocked.Exchange(ref _approximateSizeInBytes, 0);
    }

    /// <summary>
    ///     Test support: synchronously drains the bounded cache's read/write buffers and applies eviction, so
    ///     size and eviction are observable immediately. No-op for the unbounded cache.
    /// </summary>
    internal void RunPendingMaintenance() => _bounded?.DoMaintenance();

    private readonly record struct CacheEntry(IPublishedContent Content, long Size);
}
