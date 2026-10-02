namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Provides cache synchronization capabilities for load-balanced Umbraco environments.
/// </summary>
/// <remarks>
/// This service synchronizes isolated caches across servers in a load-balanced cluster by rolling forward
/// out-of-date caches. It separates synchronization into two distinct operations: internal isolated caches
/// (repositories and services) and published content caches, enabling selective cache refreshing.
/// </remarks>
public interface ICacheSyncService
{
    /// <summary>
    /// Synchronizes all caches including both isolated caches and published content caches.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method clears all isolated caches (repositories and services) and published content caches
    /// (IPublishedContentCache, route caching, etc.) to ensure complete cache consistency across the cluster.
    /// </remarks>
    void SyncAll(CancellationToken cancellationToken);

    /// <summary>
    /// Synchronizes only isolated caches without affecting the published content cache layer.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method clears only the isolated caches used by repositories and services, leaving the
    /// published content cache layer to <see cref="SyncAll" />. Payload instructions run only the in-memory
    /// <see cref="IJsonCacheRefresher.RefreshInternal(string)" />, and id-based instructions run their in-memory refresh,
    /// so no in-process or distributed locks are taken and it is safe to call while holding distributed locks; the
    /// caller's next repository read reloads the cleared entries from the database.
    /// </remarks>
    void SyncInternal(CancellationToken cancellationToken);
}
