using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Attempts to serve a published content/media item from the synchronous in-memory (L0) cache.
/// </summary>
/// <param name="key">The unique key of the item.</param>
/// <param name="content">The cached item when a hit was made; otherwise <c>null</c>.</param>
/// <returns><c>true</c> if served from the in-memory cache; otherwise <c>false</c>.</returns>
internal delegate bool TryGetCachedDelegate(Guid key, out IPublishedContent? content);

/// <summary>
/// Materialises a set of keys that missed the in-memory cache, batching any database read behind
/// a single query rather than one round trip per key.
/// </summary>
/// <param name="keys">The keys to materialise (already known to have missed L0).</param>
/// <returns>The items that exist, in the same order as <paramref name="keys"/> (missing items omitted).</returns>
internal delegate IReadOnlyList<IPublishedContent> MaterialiseMissesDelegate(IReadOnlyList<Guid> keys);

/// <summary>
/// Lazily materialises a sequence of content/media keys into <see cref="IPublishedContent"/>, pulling
/// keys in growing chunks so that short-circuiting consumers stay cheap while a full enumeration of a
/// cold set collapses its database access into a handful of batched reads.
/// </summary>
/// <remarks>
/// <para>
/// For each chunk a synchronous L0 pass (<see cref="TryGetCachedDelegate"/>) runs first; a chunk whose
/// items are all cached is yielded without any asynchronous or batched work — identical to the per-key
/// warm path. Only when a chunk contains L0 misses is <see cref="MaterialiseMissesDelegate"/> invoked
/// once for those misses (which does the L1/L2 probe and the single batched database read).
/// </para>
/// <para>
/// Chunk size starts at 1 and doubles up to <see cref="MaxChunkSize"/>. So a <c>FirstOrDefault()</c>
/// materialises a single item, a full enumeration of N items uses O(log N + N / cap) chunks, and cold
/// over-fetch on a predicate short-circuit is bounded to roughly twice what the consumer draws.
/// </para>
/// </remarks>
internal static class ChunkedPublishedContentEnumerator
{
    private const int MaxChunkSize = 256;

    public static IEnumerable<IPublishedContent> Enumerate(
        IEnumerable<Guid> keys,
        TryGetCachedDelegate tryGetCached,
        MaterialiseMissesDelegate materialiseMisses,
        Func<IPublishedContent, bool>? predicate)
    {
        var chunkSize = 1;
        var buffer = new List<Guid>(MaxChunkSize);

        using IEnumerator<Guid> enumerator = keys.GetEnumerator();

        while (true)
        {
            buffer.Clear();
            while (buffer.Count < chunkSize && enumerator.MoveNext())
            {
                buffer.Add(enumerator.Current);
            }

            if (buffer.Count == 0)
            {
                yield break;
            }

            // Synchronous L0 pass: hits fill their slot directly; misses are collected for a single
            // batched materialisation. An all-hit chunk skips the async/batched path entirely.
            var slots = new IPublishedContent?[buffer.Count];
            List<Guid>? misses = null;
            for (var i = 0; i < buffer.Count; i++)
            {
                if (tryGetCached(buffer[i], out IPublishedContent? cached) && cached is not null)
                {
                    slots[i] = cached;
                }
                else
                {
                    (misses ??= []).Add(buffer[i]);
                }
            }

            if (misses is not null)
            {
                IReadOnlyList<IPublishedContent> fetched = materialiseMisses(misses);
                if (fetched.Count > 0)
                {
                    var byKey = new Dictionary<Guid, IPublishedContent>(fetched.Count);
                    foreach (IPublishedContent item in fetched)
                    {
                        byKey[item.Key] = item;
                    }

                    for (var i = 0; i < buffer.Count; i++)
                    {
                        if (slots[i] is null && byKey.TryGetValue(buffer[i], out IPublishedContent? item))
                        {
                            slots[i] = item;
                        }
                    }
                }
            }

            for (var i = 0; i < buffer.Count; i++)
            {
                IPublishedContent? item = slots[i];
                if (item is not null && (predicate is null || predicate(item)))
                {
                    yield return item;
                }
            }

            // A short read means the source is exhausted, so there is no further chunk to grow into.
            if (buffer.Count < chunkSize)
            {
                yield break;
            }

            chunkSize = Math.Min(chunkSize * 2, MaxChunkSize);
        }
    }
}
