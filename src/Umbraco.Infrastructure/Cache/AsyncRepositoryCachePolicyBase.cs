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
/// <typeparam name="TKey">The type of the entity key.</typeparam>
public abstract class AsyncRepositoryCachePolicyBase<TEntity, TKey> : IAsyncRepositoryCachePolicy<TEntity, TKey>
    where TEntity : class, IEntity
{
    private readonly IAppPolicyCache _globalCache;
    private readonly IScopeAccessor _scopeAccessor;
    private readonly IRepositoryCacheVersionService _cacheVersionService;
    private readonly ICacheSyncService _cacheSyncService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncRepositoryCachePolicyBase{TEntity, TKey}"/> class.
    /// </summary>
    /// <param name="globalCache">The global application policy cache.</param>
    /// <param name="scopeAccessor">The scope accessor for accessing the current scope.</param>
    /// <param name="cacheVersionService">The service for managing cache version synchronization.</param>
    /// <param name="cacheSyncService">The service for synchronizing cache changes across servers.</param>
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

    /// <summary>
    ///     Gets the application policy cache, selecting global, scoped, or no-op based on the ambient scope's cache mode.
    /// </summary>
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
    public abstract Task<TEntity?> GetAsync(TKey? key, Func<TKey?, Task<TEntity?>> performGet, Func<Task<IEnumerable<TEntity>?>> performGetAll);

    /// <inheritdoc />
    public abstract Task<TEntity?> GetCachedAsync(TKey key);

    /// <inheritdoc />
    public abstract Task<bool> ExistsAsync(TKey key, Func<TKey, Task<bool>> performExists, Func<Task<IEnumerable<TEntity>?>> performGetAll);

    /// <inheritdoc />
    public abstract Task CreateAsync(TEntity entity, Func<TEntity, Task> persistNew);

    /// <inheritdoc />
    public abstract Task UpdateAsync(TEntity entity, Func<TEntity, Task> persistUpdated);

    /// <inheritdoc />
    public abstract Task DeleteAsync(TEntity entity, Func<TEntity, Task> persistDeleted);

    /// <inheritdoc />
    public abstract Task<TEntity[]> GetAllAsync(Func<Task<IEnumerable<TEntity>?>> performGetAll);

    /// <inheritdoc />
    public abstract Task<TEntity[]> GetManyAsync(TKey[] keys, Func<TKey[], Task<IEnumerable<TEntity>?>> performGetMany, Func<Task<IEnumerable<TEntity>?>> performGetAll);

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
