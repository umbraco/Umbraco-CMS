using Microsoft.Extensions.Caching.Hybrid;

namespace Umbraco.Cms.Infrastructure.HybridCache.Extensions;

/// <summary>
/// Provides extension methods on <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>.
/// </summary>
internal static class HybridCacheExtensions
{
    // Per-key locks ensuring the GetOrCreateAsync + RemoveAsync sequence executes atomically for a given cache key.
    // An entry is reference counted and only removed once no caller holds or waits on it: removing it any earlier
    // lets a later caller create a second lock for the same key and run concurrently with the current holder.
    // A plain Dictionary guarded by a lock is used, rather than a ConcurrentDictionary, because the lookup/insert/removal
    // and the reference count change must happen as one atomic step.
    private static readonly Dictionary<string, KeyLock> _keyLocks = new();

    // A probe must not write its temporary entry to the distributed cache: HybridCache completes that write in the
    // background, so it can land after the RemoveAsync below and leave the entry behind for other readers.
    private static readonly HybridCacheEntryOptions _probeEntryOptions = new()
    {
        Flags = HybridCacheEntryFlags.DisableDistributedCacheWrite,
    };

    /// <summary>
    /// Returns true if the cache contains an item with a matching key.
    /// </summary>
    /// <param name="cache">An instance of <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/></param>
    /// <param name="key">The name (key) of the item to search for in the cache.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the item exists already. False if it doesn't.</returns>
    /// <remarks>
    /// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191
    /// Leaves the cache as it found it, but is not a read-only operation: on a miss it creates a local entry
    /// and then removes it again, so a probe against a distributed L2 costs a delete.
    /// </remarks>
    public static async Task<bool> ExistsAsync<T>(this Microsoft.Extensions.Caching.Hybrid.HybridCache cache, string key, CancellationToken token)
    {
        (bool exists, _) = await TryGetValueAsync<T>(cache, key, token).ConfigureAwait(false);
        return exists;
    }

    /// <summary>
    /// Returns true if the cache contains an item with a matching key, along with the value of the matching cache entry.
    /// </summary>
    /// <typeparam name="T">The type of the value of the item in the cache.</typeparam>
    /// <param name="cache">An instance of <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/></param>
    /// <param name="key">The name (key) of the item to search for in the cache.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A tuple of <see cref="bool"/> and the object (if found) retrieved from the cache.</returns>
    /// <remarks>
    /// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191
    /// Leaves the cache as it found it, but is not a read-only operation: on a miss it creates a local entry
    /// and then removes it again, so a probe against a distributed L2 costs a delete.
    /// </remarks>
    public static async Task<(bool Exists, T? Value)> TryGetValueAsync<T>(this Microsoft.Extensions.Caching.Hybrid.HybridCache cache, string key, CancellationToken token)
    {
        var exists = true;

        // Acquire a per-key lock so that GetOrCreateAsync and the possible RemoveAsync complete without another
        // probe retrieving/creating the same key in-between. Concurrent GetOrCreateAsync calls for one key join a
        // single factory invocation, so a probe that joined another's would never learn the key was missing.
        KeyLock keyLock = AcquireKeyLockReference(key);
        try
        {
            await keyLock.Semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                T? result = await cache.GetOrCreateAsync<T?>(
                    key,
                    cancellationToken =>
                    {
                        exists = false;
                        return default;
                    },
                    _probeEntryOptions,
                    null,
                    token).ConfigureAwait(false);

                // In checking for the existence of the item, if not found, we will have created a cache entry with a null value.
                // So remove it again. Because we're holding the per-key lock there is no chance another probe
                // will observe the temporary entry between GetOrCreateAsync and RemoveAsync.
                if (exists is false)
                {
                    await cache.RemoveAsync(key).ConfigureAwait(false);
                }

                return (exists, result);
            }
            finally
            {
                keyLock.Semaphore.Release();
            }
        }
        finally
        {
            ReleaseKeyLockReference(key, keyLock);
        }
    }

    /// <summary>
    /// Gets whether a lock is currently held or awaited for the provided key. Internal for test purposes.
    /// </summary>
    internal static bool HasKeyLock(string key)
    {
        lock (_keyLocks)
        {
            return _keyLocks.ContainsKey(key);
        }
    }

    private static KeyLock AcquireKeyLockReference(string key)
    {
        lock (_keyLocks)
        {
            if (_keyLocks.TryGetValue(key, out KeyLock? keyLock) is false)
            {
                keyLock = new KeyLock();
                _keyLocks[key] = keyLock;
            }

            keyLock.ReferenceCount++;
            return keyLock;
        }
    }

    private static void ReleaseKeyLockReference(string key, KeyLock keyLock)
    {
        lock (_keyLocks)
        {
            if (--keyLock.ReferenceCount == 0)
            {
                _keyLocks.Remove(key);
            }
        }
    }

    /// <summary>
    /// A reference-counted lock serializing existence probes for a single cache key.
    /// </summary>
    /// <remarks>
    /// One instance exists per key while any caller holds or waits on it, and is discarded once the last of them
    /// releases its reference.
    /// </remarks>
    private sealed class KeyLock
    {
        /// <summary>
        /// Gets the semaphore admitting one probe at a time for the key.
        /// </summary>
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        /// <summary>
        /// Gets or sets the number of callers currently holding or waiting on <see cref="Semaphore"/>.
        /// </summary>
        /// <remarks>
        /// Only read or written while holding the lock on <c>_keyLocks</c>, so that a change to the count and the
        /// matching insert into or removal from the map happen as one atomic step.
        /// </remarks>
        public int ReferenceCount { get; set; }
    }
}
