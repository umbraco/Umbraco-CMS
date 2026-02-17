// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A base class for repository cache policies.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public abstract class AsyncRepositoryCachePolicyBase<TEntity, TId> : IAsyncRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    private readonly IAppPolicyCache _globalCache;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly IRepositoryCacheVersionService _cacheVersionService;
    private readonly ICacheSyncService _cacheSyncService;

    protected AsyncRepositoryCachePolicyBase(
        IAppPolicyCache globalCache,
        IScopeAccessor scopeAccessor,
        IRepositoryCacheVersionService cacheVersionService,
        ICacheSyncService cacheSyncService)
    {
        _globalCache = globalCache ?? throw new ArgumentNullException(nameof(globalCache));
        _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
        _cacheVersionService = cacheVersionService ?? throw new ArgumentNullException(nameof(cacheVersionService));
        _cacheSyncService = cacheSyncService ?? throw new ArgumentNullException(nameof(cacheSyncService));
    }

    protected IAppPolicyCache Cache
    {
        get
        {
            ICoreScope? ambientScope = _scopeAccessor.AmbientScope;
            switch (ambientScope?.RepositoryCacheMode)
            {
                case RepositoryCacheMode.Default:
                    return _globalCache;
                case RepositoryCacheMode.Scoped:
                    return ambientScope.IsolatedCaches.GetOrCreate<TEntity>();
                case RepositoryCacheMode.None:
                    return NoAppCache.Instance;
                default:
                    throw new NotSupportedException(
                        $"Repository cache mode {ambientScope?.RepositoryCacheMode} is not supported.");
            }
        }
    }

    /// <inheritdoc />
    public abstract Task<TEntity?> GetAsync(TId? id, Func<TId?, Task<TEntity?>> performGet);

    /// <inheritdoc />
    public abstract Task<TEntity?> GetCachedAsync(TId id);

    /// <inheritdoc />
    public abstract Task<bool> ExistsAsync(TId id, Func<TId, Task<bool>> performExists);

    /// <inheritdoc />
    public abstract Task CreateAsync(TEntity entity, Func<TEntity, Task> persistNew);

    /// <inheritdoc />
    public abstract Task UpdateAsync(TEntity entity, Func<TEntity, Task> persistUpdated);

    /// <inheritdoc />
    public abstract Task DeleteAsync(TEntity entity, Func<TEntity, Task> persistDeleted);

    /// <inheritdoc />
    public abstract Task<TEntity[]> GetAllAsync(TId[]? ids, Func<TId[]?, Task<IEnumerable<TEntity>?>> performGetAll);

    /// <inheritdoc />
    public abstract Task ClearAllAsync();

    /// <summary>
    /// Ensures that the cache is synced with the database.
    /// </summary>
    protected async Task EnsureCacheIsSyncedAsync()
    {
        var synced = await _cacheVersionService.IsCacheSyncedAsync<TEntity>();
        if (synced)
        {
            return;
        }

        _cacheSyncService.SyncInternal(CancellationToken.None);
    }

    /// <summary>
    /// Registers a change in the cache.
    /// </summary>
    protected async Task RegisterCacheChangeAsync() => await _cacheVersionService.SetCacheUpdatedAsync<TEntity>();
}
