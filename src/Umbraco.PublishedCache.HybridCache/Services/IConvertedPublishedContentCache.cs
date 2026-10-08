using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
///     The in-process (L0) cache of converted <see cref="IPublishedElement" />, behind a single
///     insert/remove/clear path that also tracks the entry count and an approximate retained byte total.
/// </summary>
/// <remarks>
///     Implemented by an unbounded store (the historical behaviour) and, when the bounded cache package is
///     installed and a maximum is configured, a bounded scan-resistant store. The implementation is chosen by
///     <see cref="IConvertedPublishedContentCacheFactory" />.
/// </remarks>
/// <typeparam name="TKey">The cache key type (string for documents/elements, Guid for media).</typeparam>
/// <typeparam name="TValue">
///     The cached converted type (<see cref="IPublishedContent" /> for documents and media,
///     <see cref="IPublishedElement" /> for elements).
/// </typeparam>
internal interface IConvertedPublishedContentCache<TKey, TValue>
    where TKey : notnull
    where TValue : class, IPublishedElement
{
    /// <summary>
    ///     Gets the number of entries currently held.
    /// </summary>
    long Count { get; }

    /// <summary>
    ///     Gets the approximate retained size, in bytes, of the cached entries.
    /// </summary>
    long ApproximateSizeInBytes { get; }

    /// <summary>
    ///     Attempts to get a cached converted content item.
    /// </summary>
    bool TryGet(TKey key, out TValue? content);

    /// <summary>
    ///     Adds or replaces a cached converted content item.
    /// </summary>
    void Set(TKey key, TValue content, long approximateSizeInBytes);

    /// <summary>
    ///     Removes a cached entry.
    /// </summary>
    bool Remove(TKey key);

    /// <summary>
    ///     Removes every entry whose content matches the predicate.
    /// </summary>
    void RemoveWhere(Func<TValue, bool> predicate);

    /// <summary>
    ///     Removes all entries.
    /// </summary>
    void Clear();
}
