namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Estimates the total size of a large collection by sizing a bounded sample and extrapolating across the
/// full count, so a per-tick size diagnostic does not pay an O(n) cost on very large caches.
/// </summary>
internal static class SampledSizeEstimator
{
    /// <summary>
    /// Sizes up to <paramref name="maxSample" /> items from <paramref name="items" /> and scales the sampled
    /// average across <paramref name="count" />.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="count">The total number of items in the collection.</param>
    /// <param name="items">The items to sample (enumerated lazily; only the first <paramref name="maxSample" /> are read).</param>
    /// <param name="sizeOf">Returns the approximate size, in bytes, of a single item.</param>
    /// <param name="maxSample">The maximum number of items to size before extrapolating.</param>
    /// <returns>The extrapolated approximate total size in bytes.</returns>
    public static long Estimate<T>(int count, IEnumerable<T> items, Func<T, long> sizeOf, int maxSample = 1000)
    {
        if (count == 0)
        {
            return 0;
        }

        long sampled = 0;
        long sampledBytes = 0;
        foreach (T item in items)
        {
            sampledBytes += sizeOf(item);
            if (++sampled >= maxSample)
            {
                break;
            }
        }

        return sampled == 0 ? 0 : count * (sampledBytes / sampled);
    }
}
