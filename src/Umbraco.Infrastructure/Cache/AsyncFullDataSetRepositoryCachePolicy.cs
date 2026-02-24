using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Cache;

/// <summary>
///     Represents an async caching policy that caches the entire entities set as a single collection.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the identifier.</typeparam>
internal sealed class AsyncFullDataSetRepositoryCachePolicy<TEntity, TId> : AsyncRepositoryCachePolicyBase<TEntity, TId>
    where TEntity : class, IEntity
{
    private readonly Func<TEntity, TId> _entityGetId;

    private readonly bool _expires;
    private readonly SemaphoreSlim _getAllLock = new(1, 1);

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncFullDataSetRepositoryCachePolicy{TEntity, TId}"/> class.
    /// </summary>
    /// <param name="globalCache">The global application policy cache.</param>
    /// <param name="scopeAccessor">The scope accessor for accessing the current scope.</param>
    /// <param name="cacheVersionService">The service for managing cache version synchronization.</param>
    /// <param name="cacheSyncService">The service for synchronizing cache changes across servers.</param>
    /// <param name="entityGetId">A function to extract the identifier from an entity.</param>
    /// <param name="expires">Whether cached items should expire after a timeout.</param>
    public AsyncFullDataSetRepositoryCachePolicy(
        IAppPolicyCache globalCache,
        IScopeAccessor scopeAccessor,
        IRepositoryCacheVersionService cacheVersionService,
        ICacheSyncService cacheSyncService,
        Func<TEntity, TId> entityGetId,
        bool expires)
        : base(globalCache, scopeAccessor, cacheVersionService, cacheSyncService)
    {
        _entityGetId = entityGetId;
        _expires = expires;
    }

    private static string GetEntityTypeCacheKey() => RepositoryCacheKeys.GetKey<TEntity>();

    private void InsertEntities(TEntity[]? entities)
    {
        if (entities is null)
        {
            return;
        }

        // cache is expected to be a deep-cloning cache ie it deep-clones whatever is
        // IDeepCloneable when it goes in, and out. it also resets dirty properties,
        // making sure that no 'dirty' entity is cached.
        //
        // this policy is caching the entire list of entities. to ensure that entities
        // are properly deep-clones when cached, it uses a DeepCloneableList. however,
        // we don't want to deep-clone *each* entity in the list when fetching it from
        // cache as that would not be efficient for Get(id). so the DeepCloneableList is
        // set to ListCloneBehavior.CloneOnce ie it will clone *once* when inserting,
        // and then will *not* clone when retrieving.
        var key = GetEntityTypeCacheKey();

        if (_expires)
        {
            Cache.Insert(key, () => new DeepCloneableList<TEntity>(entities), RepositoryCacheConstants.DefaultCacheDuration, true);
        }
        else
        {
            Cache.Insert(key, () => new DeepCloneableList<TEntity>(entities));
        }
    }

    /// <inheritdoc/>
    public override async Task<TEntity?> GetAsync(TId? id, Func<TId?, Task<TEntity?>> performGet, Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();

        // get all from the cache, then look for the entity
        IEnumerable<TEntity> all = await GetAllCachedAsync(performGetAll);
        TEntity? entity = all.FirstOrDefault(x => _entityGetId(x)?.Equals(id) ?? false);

        // see note in InsertEntities - what we get here is the original
        // cached entity, not a clone, so we need to manually ensure it is deep-cloned.
        return (TEntity?)entity?.DeepClone();
    }

    /// <inheritdoc/>
    public override async Task<TEntity?> GetCachedAsync(TId id)
    {
        await EnsureCacheIsSyncedAsync();

        // get all from the cache -- and only the cache, then look for the entity
        DeepCloneableList<TEntity>? all = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetEntityTypeCacheKey());
        TEntity? entity = all?.FirstOrDefault(x => _entityGetId(x)?.Equals(id) ?? false);

        // see note in InsertEntities - what we get here is the original
        // cached entity, not a clone, so we need to manually ensure it is deep-cloned.
        return (TEntity?)entity?.DeepClone();
    }

    /// <inheritdoc/>
    public override async Task<bool> ExistsAsync(TId id, Func<TId, Task<bool>> performExists, Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();

        // get all as one set, then look for the entity
        IEnumerable<TEntity> all = await GetAllCachedAsync(performGetAll);
        return all.Any(x => _entityGetId(x)?.Equals(id) ?? false);
    }

    /// <inheritdoc/>
    public override async Task CreateAsync(TEntity entity, Func<TEntity, Task> persistNew)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            await persistNew(entity);
        }
        finally
        {
            await ClearAllAsync();
        }
    }

    /// <inheritdoc/>
    public override async Task UpdateAsync(TEntity entity, Func<TEntity, Task> persistUpdated)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            await persistUpdated(entity);
        }
        finally
        {
            await ClearAllAsync();
        }

        // We've changed the entity, register cache change for other servers.
        // We assume that if something goes wrong, we'll roll back, so don't need to register the change.
        await RegisterCacheChangeAsync();
    }

    /// <inheritdoc/>
    public override async Task DeleteAsync(TEntity entity, Func<TEntity, Task> persistDeleted)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            await persistDeleted(entity);
        }
        finally
        {
            await ClearAllAsync();
        }

        // We've changed the entity, register cache change for other servers.
        // We assume that if something goes wrong, we'll roll back, so don't need to register the change.
        await RegisterCacheChangeAsync();
    }

    /// <inheritdoc/>
    public override async Task<TEntity[]> GetAllAsync(Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();

        // get all as one set, from cache if possible, else repo
        IEnumerable<TEntity> all = await GetAllCachedAsync(performGetAll);

        // and return
        // see note in SetCacheActionToInsertEntities - what we get here is the original
        // cached entities, not clones, so we need to manually ensure they are deep-cloned.
        return all.Select(x => (TEntity)x.DeepClone()).ToArray();
    }

    /// <inheritdoc/>
    public override async Task<TEntity[]> GetManyAsync(TId[] ids, Func<TId[], Task<IEnumerable<TEntity>?>> performGetMany, Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();

        // get all as one set, from cache if possible, else repo
        IEnumerable<TEntity> all = (await GetAllCachedAsync(performGetAll)).Where(x => ids.Contains(_entityGetId(x)));

        // see note in SetCacheActionToInsertEntities - what we get here is the original
        // cached entities, not clones, so we need to manually ensure they are deep-cloned.
        return all.Select(x => (TEntity)x.DeepClone()).ToArray();
    }

    /// <inheritdoc/>
    public override Task ClearAllAsync()
    {
        Cache.Clear(GetEntityTypeCacheKey());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all cached entities, or retrieves them from the repository if not cached.
    /// </summary>
    /// <remarks>
    /// Uses double-check locking to prevent the "thundering herd" problem where multiple
    /// threads detecting a cache miss would all query the database simultaneously.
    /// Does NOT clone anything, so be nice with the returned values.
    /// </remarks>
    /// <param name="performGetAll">Method which retrieves all items.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal async Task<IEnumerable<TEntity>> GetAllCachedAsync(Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        // Fast path - check cache without lock.
        DeepCloneableList<TEntity>? all = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetEntityTypeCacheKey());
        if (all is not null)
        {
            return all.ToArray();
        }

        // Slow path - lock to prevent thundering herd on cache miss.
        await _getAllLock.WaitAsync();
        try
        {
            // Double-check inside lock - another thread may have populated the cache.
            all = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetEntityTypeCacheKey());
            if (all != null)
            {
                return all.ToArray();
            }

            // Only one thread queries the database.
            TEntity[]? entities = (await performGetAll())?.WhereNotNull().ToArray();
            InsertEntities(entities); // may be an empty array...
            return entities ?? Enumerable.Empty<TEntity>();
        }
        finally
        {
            _getAllLock.Release();
        }
    }
}
