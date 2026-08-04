namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Resolves as many of <paramref name="keys"/> as this tier can, batched into a single call, keyed by
/// the key each item was resolved from.
/// </summary>
/// <param name="keys">The candidate keys for this tier (already known to have missed every earlier tier).</param>
/// <returns>The items this tier resolved, keyed by the key each one was resolved from.</returns>
internal delegate IReadOnlyDictionary<TKey, TItem> GetItemsDelegate<TKey, TItem>(IReadOnlyCollection<TKey> keys)
    where TKey : notnull;

/// <summary>
/// Lazily materialises a sequence of keys into items, pulling keys in growing chunks so that
/// short-circuiting consumers stay cheap while a full enumeration of a cold set collapses its expensive
/// work into a handful of batched calls.
/// </summary>
/// <remarks>
/// <para>
/// Each chunk is run through the tiers given to <see cref="Enumerate{TKey,TItem}"/>, in order — e.g. a
/// cheap synchronous L0 probe first, then a batched database read for whatever L0 missed. A tier is only
/// invoked for the keys the previous tiers could not resolve, and a chunk fully resolved by an earlier
/// tier skips the later ones entirely — identical to the per-key warm path when everything is cached.
/// </para>
/// <para>
/// Chunk size starts at 1 and doubles up to <see cref="MaxChunkSize"/>. So a <c>FirstOrDefault()</c>
/// materialises a single item, a full enumeration of N items uses O(log N + N / cap) chunks, and cold
/// over-fetch on a predicate short-circuit is bounded to roughly twice what the consumer draws.
/// </para>
/// </remarks>
internal static class ChunkedTieredEnumerator
{
    private const int MaxChunkSize = 256;

    /// <summary>
    /// Lazily materialises <paramref name="keys"/> into <typeparamref name="TItem"/> in growing chunks,
    /// running each chunk through <paramref name="firstTier"/> and then <paramref name="additionalTiers"/>,
    /// in order, until every key is resolved or every tier has been tried.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to look up and deduplicate items.</typeparam>
    /// <typeparam name="TItem">The type of item being materialised.</typeparam>
    /// <param name="keys">The keys to materialise, in the order they should be yielded. Duplicates are collapsed to a single result.</param>
    /// <param name="firstTier">The first tier to run for each chunk.</param>
    /// <param name="additionalTiers">Further tiers, in order, for whatever <paramref name="firstTier"/> and the ones before them left unresolved.</param>
    /// <returns>The resolved items, in input order, with missing items omitted. Apply any further filtering with <c>.Where()</c> on the result.</returns>
    public static IEnumerable<TItem> Enumerate<TKey, TItem>(
        IEnumerable<TKey> keys,
        GetItemsDelegate<TKey, TItem> firstTier,
        params GetItemsDelegate<TKey, TItem>[] additionalTiers)
        where TKey : notnull
    {
        GetItemsDelegate<TKey, TItem>[] tiers = additionalTiers.Length == 0
            ? [firstTier]
            : [firstTier, .. additionalTiers];

        var chunkSize = 1;
        var chunk = new List<TKey>(MaxChunkSize);
        using IEnumerator<TKey> enumerator = keys.Distinct().GetEnumerator();

        while (TryFillChunk(enumerator, chunk, chunkSize))
        {
            foreach (TItem item in ResolveChunk(chunk, tiers))
            {
                yield return item;
            }

            // A short read means the source is exhausted, so there is no further chunk to grow into.
            if (chunk.Count < chunkSize)
            {
                yield break;
            }

            chunkSize = Math.Min(chunkSize * 2, MaxChunkSize);
        }
    }

    /// <summary>
    /// Refills the reusable <paramref name="chunk"/> with up to <paramref name="chunkSize"/> keys from the source.
    /// </summary>
    /// <returns><c>true</c> if the chunk holds at least one key; <c>false</c> once the source is exhausted.</returns>
    private static bool TryFillChunk<TKey>(IEnumerator<TKey> enumerator, List<TKey> chunk, int chunkSize)
    {
        chunk.Clear();
        while (chunk.Count < chunkSize && enumerator.MoveNext())
        {
            chunk.Add(enumerator.Current);
        }

        return chunk.Count > 0;
    }

    /// <summary>
    /// Resolves one chunk (already deduplicated by <see cref="Enumerate{TKey,TItem}"/>) to its items in
    /// chunk order, running <paramref name="tryGetItems"/> in turn against whatever the previous tier left
    /// unresolved. A tier is skipped once nothing remains unresolved, so a chunk fully resolved by an
    /// early tier never reaches a later one.
    /// </summary>
    private static List<TItem> ResolveChunk<TKey, TItem>(
        List<TKey> chunk,
        GetItemsDelegate<TKey, TItem>[] tryGetItems)
        where TKey : notnull
    {
        var resolvedByKey = new Dictionary<TKey, TItem>(chunk.Count);
        IReadOnlyCollection<TKey> pending = chunk;

        foreach (GetItemsDelegate<TKey, TItem> tryGetTier in tryGetItems)
        {
            if (pending.Count == 0)
            {
                break;
            }

            foreach (KeyValuePair<TKey, TItem> pair in tryGetTier(pending))
            {
                resolvedByKey[pair.Key] = pair.Value;
            }

            pending = pending.Where(key => !resolvedByKey.ContainsKey(key)).ToArray();
        }

        var resolved = new List<TItem>(chunk.Count);
        foreach (TKey key in chunk)
        {
            if (resolvedByKey.TryGetValue(key, out TItem? item))
            {
                resolved.Add(item);
            }
        }

        return resolved;
    }
}
