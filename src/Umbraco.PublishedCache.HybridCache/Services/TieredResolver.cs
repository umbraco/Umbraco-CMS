namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
/// Resolves as many of <paramref name="keys"/> as this tier can, batched into a single asynchronous
/// call, writing each one it resolves into <paramref name="results"/> keyed by the key it was resolved
/// from.
/// </summary>
/// <param name="keys">The candidate keys for this tier (already known to have missed every earlier tier).</param>
/// <param name="results">The dictionary shared across every tier for this call; write resolved keys into it rather than returning a new dictionary.</param>
internal delegate Task GetItemsAsyncDelegate<TKey, TItem>(IReadOnlyCollection<TKey> keys, IDictionary<TKey, TItem> results)
    where TKey : notnull;

/// <summary>
/// Eagerly resolves a bounded, already-known set of keys into items by running a set of asynchronous
/// tiers in order, each one only asked about whatever the previous tiers could not resolve.
/// </summary>
/// <remarks>
/// Used by <see cref="DocumentCacheService"/> and <see cref="MediaCacheService"/> for their
/// <c>GetByKeysAsync</c> batch lookups — e.g. a synchronous L0 probe first, then a batched database
/// read for whatever L0 missed. Deliberately not shared with <c>Umbraco.Core</c>'s sync
/// <c>ChunkedTieredResolver</c>: HybridCache and Core are versioned and released as separate NuGet packages
/// with a floating dependency range within a major version (e.g. <c>[17.5.3, 18.0.0)</c>), so an
/// internal type exposed across that boundary via <c>InternalsVisibleTo</c> would carry no
/// binary-compatibility guarantee — a Core patch could change an internal signature with nothing to
/// stop a resolved-but-mismatched HybridCache version from still calling the old one. Keeping this
/// resolver local to the one assembly that uses it avoids that entirely.
/// </remarks>
internal static class TieredResolver
{
    /// <summary>
    /// Eagerly resolves every key in <paramref name="keys"/> by running <paramref name="firstTier"/> and
    /// then <paramref name="additionalTiers"/>, in order, until every key is resolved or every tier has
    /// been tried.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to look up items.</typeparam>
    /// <typeparam name="TItem">The type of item being resolved.</typeparam>
    /// <param name="keys">The keys to resolve, in the order they should appear in the result. A key repeated in <paramref name="keys"/> resolves to the same item at every occurrence.</param>
    /// <param name="firstTier">The first tier to run.</param>
    /// <param name="additionalTiers">Further tiers, in order, for whatever <paramref name="firstTier"/> and the ones before them left unresolved.</param>
    /// <returns>The resolved items, in input order (including repeats), with missing items omitted.</returns>
    public static async Task<IReadOnlyList<TItem>> ResolveAsync<TKey, TItem>(
        IReadOnlyCollection<TKey> keys,
        GetItemsAsyncDelegate<TKey, TItem> firstTier,
        params GetItemsAsyncDelegate<TKey, TItem>[] additionalTiers)
        where TKey : notnull
    {
        GetItemsAsyncDelegate<TKey, TItem>[] tiers = additionalTiers.Length == 0
            ? [firstTier]
            : [firstTier, .. additionalTiers];

        var resolvedByKey = new Dictionary<TKey, TItem>(keys.Count);
        IReadOnlyCollection<TKey> pending = [.. keys.Distinct()];

        foreach (GetItemsAsyncDelegate<TKey, TItem> tryGetTier in tiers)
        {
            if (pending.Count == 0)
            {
                break;
            }

            await tryGetTier(pending, resolvedByKey);

            pending = pending.Where(key => !resolvedByKey.ContainsKey(key)).ToArray();
        }

        // Keys are not deduplicated in the result: a batched lookup is expected to return one item per
        // requested occurrence, so a repeated key resolves to the same item at every occurrence rather
        // than collapsing to a single result.
        var resolved = new List<TItem>(keys.Count);
        foreach (TKey key in keys)
        {
            if (resolvedByKey.TryGetValue(key, out TItem? item))
            {
                resolved.Add(item);
            }
        }

        return resolved;
    }
}
