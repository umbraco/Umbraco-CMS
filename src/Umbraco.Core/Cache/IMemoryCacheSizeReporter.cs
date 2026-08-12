namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Reports the approximate size of an in-memory cache, for diagnostics and observability.
/// </summary>
/// <remarks>
/// Implemented by in-memory caches whose footprint scales with the size of the content tree, so their
/// retained entry count can be logged (e.g. by a periodic diagnostics job) during full-tree operations
/// such as reindexing or crawling the published site.
/// <para>
/// The reported value is an approximate <em>entry count</em>, not a byte measurement: per-entry size
/// varies widely, so the count is intended as a <em>trend</em> signal (a count that grows during a
/// tree-walk and never falls indicates unbounded retention) and for <em>attribution</em> (which cache is
/// largest when the process heap grows), rather than as an absolute memory figure. Absolute bytes are
/// obtained from process-level totals (managed heap / working set) and a GC dump. The count is read
/// without locking.
/// </para>
/// </remarks>
public interface IMemoryCacheSizeReporter
{
    /// <summary>
    /// Gets a human-readable name identifying the cache in diagnostic output.
    /// </summary>
    string CacheName { get; }

    /// <summary>
    /// Gets the approximate number of entries currently retained in the cache.
    /// </summary>
    /// <returns>The approximate entry count.</returns>
    long GetApproximateCount();

    /// <summary>
    /// Gets an approximate retained size of the cache in bytes, or <c>null</c> when the cache cannot be
    /// cheaply sized.
    /// </summary>
    /// <remarks>
    /// Where provided, this is a coarse estimate (underlying content / structural size, not a precise
    /// managed-heap measurement) for the same trend/attribution purpose as the entry count. Absolute bytes
    /// come from a GC dump.
    /// </remarks>
    /// <returns>The approximate size in bytes, or <c>null</c> if not available.</returns>
    long? GetApproximateBytes() => null;
}
