namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides extension methods for <see cref="IRepositoryCacheVersionService" />.
/// </summary>
internal static class RepositoryCacheVersionServiceExtensions
{
    /// <summary>
    ///     Syncs the isolated caches with the changes made by other servers when the cache for
    ///     <typeparamref name="TEntity" /> is out of date.
    /// </summary>
    /// <typeparam name="TEntity">The type of the cached entity.</typeparam>
    /// <param name="cacheVersionService">The repository cache version service.</param>
    /// <param name="cacheSyncService">The cache sync service.</param>
    internal static void EnsureCacheIsSynced<TEntity>(
        this IRepositoryCacheVersionService cacheVersionService,
        ICacheSyncService cacheSyncService)
        where TEntity : class
    {
        // TODO (V20): call IsCacheSyncedAsync once locks and notification publishing are asynchronous.
        if (cacheVersionService.IsCacheSynced<TEntity>())
        {
            return;
        }

        cacheSyncService.SyncInternal(CancellationToken.None);
    }
}
