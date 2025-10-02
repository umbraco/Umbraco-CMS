namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Provides methods to manage and validate cache versioning for repository entities,
/// ensuring cache consistency with the underlying database.
/// </summary>
public interface IRepositoryCacheVersionService
{
    /// <summary>
    /// Validates if the cache is synced with the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of the cached entity.</typeparam>
    /// <returns>True if cache is synced, false if cache needs fast-forwarding.</returns>
    Task<bool> IsCacheSyncedAsync<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Registers a cache update for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the cached entity.</typeparam>
    Task SetCacheUpdatedAsync<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Registers that the cache has been synced with the database.
    /// </summary>
    Task SetCachesSyncedAsync(); // TODO: Set caches synced when they are syncde
}
