using Umbraco.Cms.Core.Models;

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
    /// <remarks>
    /// Other servers detect the update through the published cache version. An implementation may defer publishing
    /// it until <see cref="FlushCacheUpdatesAsync" /> is called, so that the version never becomes visible before
    /// the cache instructions that describe the change.
    /// </remarks>
    Task SetCacheUpdatedAsync<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Publishes the cache updates registered by <see cref="SetCacheUpdatedAsync{TEntity}" /> whose publication was
    /// deferred.
    /// </summary>
    /// <remarks>
    /// Called once the cache instructions for the current unit of work have been written. Implementations that
    /// publish updates immediately have nothing to do here.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    Task FlushCacheUpdatesAsync() => Task.CompletedTask;

    /// <summary>
    /// Gets the cache versions currently published for all entity types.
    /// </summary>
    /// <returns>The published cache versions; a version whose value is <see langword="null" /> has never been published.</returns>
    // TODO (V19): Remove the default implementation.
    Task<IReadOnlyCollection<RepositoryCacheVersion>> GetCacheVersionsAsync()
        => Task.FromResult<IReadOnlyCollection<RepositoryCacheVersion>>(Array.Empty<RepositoryCacheVersion>());

    /// <summary>
    /// Registers that the cache has been synced with the database.
    /// </summary>
    /// <remarks>
    /// Adopts the versions currently published. Prefer
    /// <see cref="SetCachesSyncedAsync(IEnumerable{RepositoryCacheVersion})" /> with versions read before the cache
    /// instructions were processed.
    /// </remarks>
    Task SetCachesSyncedAsync();

    /// <summary>
    /// Registers that the local caches have been synced up to the given cache versions.
    /// </summary>
    /// <param name="cacheVersions">
    /// The versions read before the cache instructions were processed, typically from <see cref="GetCacheVersionsAsync" />.
    /// </param>
    /// <remarks>
    /// Adopting the versions read before processing, rather than the versions published afterwards, means a version
    /// published while instructions were being processed is still seen as out of date and triggers another sync.
    /// Concurrent syncs may adopt an older set last; that costs one redundant sync and nothing more.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    Task SetCachesSyncedAsync(IEnumerable<RepositoryCacheVersion> cacheVersions) => SetCachesSyncedAsync();
}
