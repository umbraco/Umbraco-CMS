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

    /// <summary>
    /// Lazily materialises <paramref name="keys"/> into <see cref="IPublishedContent"/> in growing chunks,
    /// serving in-memory (L0) hits synchronously and batching the database read for the rest.
    /// </summary>
    /// <param name="keys">The keys to materialise, in the order they should be yielded.</param>
    /// <param name="tryGetCached">The synchronous L0 probe.</param>
    /// <param name="materialiseMisses">The batched materialiser for keys that missed L0.</param>
    /// <param name="predicate">An optional post-materialisation filter (e.g. a culture check); <c>null</c> to include all.</param>
    /// <returns>The resolved items, in input order, with missing and filtered-out items omitted.</returns>
    public static IEnumerable<IPublishedContent> Enumerate(
        IEnumerable<Guid> keys,
        TryGetCachedDelegate tryGetCached,
        MaterialiseMissesDelegate materialiseMisses,
        Func<IPublishedContent, bool>? predicate)
    {
        var chunkSize = 1;
        var buffer = new List<Guid>(MaxChunkSize);

        using IEnumerator<Guid> enumerator = keys.GetEnumerator();

        while (FillChunk(enumerator, chunkSize, buffer))
        {
            foreach (IPublishedContent item in ResolveChunk(buffer, tryGetCached, materialiseMisses))
            {
                if (predicate is null || predicate(item))
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

    // Refills the reusable buffer with up to chunkSize keys; returns false once the source is exhausted.
    private static bool FillChunk(IEnumerator<Guid> enumerator, int chunkSize, List<Guid> buffer)
    {
        buffer.Clear();
        while (buffer.Count < chunkSize && enumerator.MoveNext())
        {
            buffer.Add(enumerator.Current);
        }

        return buffer.Count > 0;
    }

    // Resolves one chunk to its items in buffer order: L0 hits served directly, the rest materialised in
    // a single batched call. An all-hit chunk never invokes the batched materialiser.
    private static List<IPublishedContent> ResolveChunk(
        List<Guid> chunk,
        TryGetCachedDelegate tryGetCached,
        MaterialiseMissesDelegate materialiseMisses)
    {
        var slots = new IPublishedContent?[chunk.Count];
        List<Guid>? misses = null;
        for (var i = 0; i < chunk.Count; i++)
        {
            if (tryGetCached(chunk[i], out IPublishedContent? cached) && cached is not null)
            {
                slots[i] = cached;
            }
            else
            {
                (misses ??= []).Add(chunk[i]);
            }
        }

        if (misses is not null)
        {
            PlaceMisses(chunk, slots, materialiseMisses(misses));
        }

        var resolved = new List<IPublishedContent>(chunk.Count);
        foreach (IPublishedContent? item in slots)
        {
            if (item is not null)
            {
                resolved.Add(item);
            }
        }

        return resolved;
    }

    // Slots the batch-materialised items back into their input positions, keyed by content key.
    private static void PlaceMisses(List<Guid> chunk, IPublishedContent?[] slots, IReadOnlyList<IPublishedContent> fetched)
    {
        if (fetched.Count == 0)
        {
            return;
        }

        var byKey = new Dictionary<Guid, IPublishedContent>(fetched.Count);
        foreach (IPublishedContent item in fetched)
        {
            byKey[item.Key] = item;
        }

        for (var i = 0; i < chunk.Count; i++)
        {
            if (slots[i] is null && byKey.TryGetValue(chunk[i], out IPublishedContent? item))
            {
                slots[i] = item;
            }
        }
    }
}
