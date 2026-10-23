using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// A simple cache version service that assumes the cache is always in sync.
/// <remarks>
/// This is useful in scenarios where you have a single server setup and do not need to manage cache synchronization across multiple servers.
/// </remarks>
/// </summary>
public class SingleServerCacheVersionService : IRepositoryCacheVersionService
{
    /// <inheritdoc />
    public Task<bool> IsCacheSyncedAsync<TEntity>()
        where TEntity : class
        => Task.FromResult(true);

    /// <inheritdoc />
    public bool IsCacheSynced<TEntity>()
        where TEntity : class
        => true;

    /// <inheritdoc />
    public Task SetCacheUpdatedAsync<TEntity>()
        where TEntity : class
        => Task.CompletedTask;

    /// <inheritdoc />
    public void SetCacheUpdated<TEntity>()
        where TEntity : class
    {
    }

    /// <inheritdoc />
    public Task FlushCacheUpdatesAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public void FlushCacheUpdates()
    {
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<RepositoryCacheVersion>> GetCacheVersionsAsync()
        => Task.FromResult<IReadOnlyCollection<RepositoryCacheVersion>>(Array.Empty<RepositoryCacheVersion>());

    /// <inheritdoc />
    public IReadOnlyCollection<RepositoryCacheVersion> GetCacheVersions() => Array.Empty<RepositoryCacheVersion>();

    /// <inheritdoc />
    public Task SetCachesSyncedAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public Task SetCachesSyncedAsync(IEnumerable<RepositoryCacheVersion> cacheVersions) => Task.CompletedTask;
}
