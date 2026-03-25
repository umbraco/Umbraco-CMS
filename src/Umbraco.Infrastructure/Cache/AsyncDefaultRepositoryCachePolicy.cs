// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;
using IScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents the default cache policy.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
/// <remarks>
///     <para>The default cache policy caches entities with a 5 minutes sliding expiration.</para>
///     <para>Each entity is cached individually.</para>
///     <para>If options.GetAllCacheAllowZeroCount then a 'zero-count' array is cached when GetAll finds nothing.</para>
///     <para>If options.GetAllCacheValidateCount then we check against the db when getting many entities.</para>
/// </remarks>
public class AsyncDefaultRepositoryCachePolicy<TEntity, TKey> : AsyncRepositoryCachePolicyBase<TEntity, TKey>
    where TEntity : class, IEntity
{
    private static readonly TEntity[] _emptyEntities = new TEntity[0]; // const
    private readonly AsyncRepositoryCachePolicyOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncDefaultRepositoryCachePolicy{TEntity, TKey}"/> class.
    /// </summary>
    /// <param name="cache">The application policy cache.</param>
    /// <param name="scopeAccessor">The scope accessor for accessing the current scope.</param>
    /// <param name="options">The cache policy options.</param>
    /// <param name="repositoryCacheVersionService">The service for managing cache version synchronization.</param>
    /// <param name="cacheSyncService">The service for synchronizing cache changes across servers.</param>
    public AsyncDefaultRepositoryCachePolicy(
        IAppPolicyCache cache,
        IScopeAccessor scopeAccessor,
        AsyncRepositoryCachePolicyOptions options,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(cache, scopeAccessor, repositoryCacheVersionService, cacheSyncService) =>
        _options = options ?? throw new ArgumentNullException(nameof(options));

    /// <summary>
    ///     Gets the cache key prefix for this entity type.
    /// </summary>
    protected string EntityTypeCacheKey { get; } = RepositoryCacheKeys.GetKey<TEntity>();

    /// <inheritdoc />
    public override async Task CreateAsync(TEntity entity, Func<TEntity, Task> persistNew)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            await persistNew(entity);

            // just to be safe, we cannot cache an item without an identity
            if (entity.HasIdentity)
            {
                Cache.Insert(GetEntityCacheKey(entity.Id), () => entity, TimeSpan.FromMinutes(5), true);
            }

            // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
            Cache.Clear(EntityTypeCacheKey);
        }
        catch
        {
            // if an exception is thrown we need to remove the entry from cache,
            // this is ONLY a work around because of the way
            // that we cache entities: http://issues.umbraco.org/issue/U4-4259
            Cache.Clear(GetEntityCacheKey(entity.Id));

            // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
            Cache.Clear(EntityTypeCacheKey);

            throw;
        }
    }

    /// <inheritdoc />
    public override async Task UpdateAsync(TEntity entity, Func<TEntity, Task> persistUpdated)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            await persistUpdated(entity);

            // just to be safe, we cannot cache an item without an identity
            if (entity.HasIdentity)
            {
                Cache.Insert(GetEntityCacheKey(entity.Id), () => entity, TimeSpan.FromMinutes(5), true);
            }

            // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
            Cache.Clear(EntityTypeCacheKey);
        }
        catch
        {
            // if an exception is thrown we need to remove the entry from cache,
            // this is ONLY a work around because of the way
            // that we cache entities: http://issues.umbraco.org/issue/U4-4259
            Cache.Clear(GetEntityCacheKey(entity.Id));

            // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
            Cache.Clear(EntityTypeCacheKey);

            throw;
        }

        // We've changed the entity, register cache change for other servers.
        // We assume that if something goes wrong, we'll roll back, so don't need to register the change.
        await RegisterCacheChangeAsync();
    }

    /// <inheritdoc />
    public override async Task DeleteAsync(TEntity entity, Func<TEntity, Task> persistDeleted)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            await persistDeleted(entity);
        }
        finally
        {
            // whatever happens, clear the cache
            var cacheKey = GetEntityCacheKey(entity.Id);

            Cache.Clear(cacheKey);

            // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
            Cache.Clear(EntityTypeCacheKey);
        }

        // We've removed an entity, register cache change for other servers.
        await RegisterCacheChangeAsync();
    }

    /// <inheritdoc />
    public override async Task<TEntity?> GetAsync(TKey? key, Func<TKey?, Task<TEntity?>> performGet, Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();

        var cacheKey = GetEntityCacheKey(key);

        TEntity? fromCache = Cache.GetCacheItem<TEntity>(cacheKey);

        // If found in cache then return immediately.
        if (fromCache is not null)
        {
            return fromCache;
        }

        // If we've cached a "null" value, return null.
        if (_options.CacheNullValues && Cache.GetCacheItem<string>(cacheKey) == Constants.Cache.NullRepresentationInCache)
        {
            return null;
        }

        // Otherwise go to the database to retrieve.
        TEntity? entity = await performGet(key);

        if (entity != null && entity.HasIdentity)
        {
            // If we've found an identified entity, cache it for subsequent retrieval.
            InsertEntity(cacheKey, entity);
        }
        else if (entity is null && _options.CacheNullValues)
        {
            // If we've not found an entity, and we're caching null values, cache a "null" value.
            InsertNull(cacheKey);
        }

        return entity;
    }

    /// <inheritdoc />
    public override async Task<TEntity?> GetCachedAsync(TKey key)
    {
        await EnsureCacheIsSyncedAsync();
        var cacheKey = GetEntityCacheKey(key);
        return Cache.GetCacheItem<TEntity>(cacheKey);
    }

    /// <inheritdoc />
    public override async Task<bool> ExistsAsync(TKey key, Func<TKey, Task<bool>> performExists, Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();

        // if found in cache the return else check
        var cacheKey = GetEntityCacheKey(key);
        TEntity? fromCache = Cache.GetCacheItem<TEntity>(cacheKey);
        return fromCache != null || await performExists(key);
    }

    /// <inheritdoc />
    public override async Task<TEntity[]> GetAllAsync(Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();

        // get everything we have
        TEntity?[] entities = Cache.GetCacheItemsByKeySearch<TEntity>(EntityTypeCacheKey)
            .ToArray(); // no need for null checks, we are not caching nulls

        if (entities.Length > 0)
        {
            // if some of them were in the cache...
            if (_options.GetAllCacheValidateCount)
            {
                // need to validate the count, get the actual count and return if ok
                if (_options.PerformCountAsync is not null)
                {
                    var totalCount = await _options.PerformCountAsync();
                    if (entities.Length == totalCount)
                    {
                        return entities.WhereNotNull().ToArray();
                    }
                }
            }
            else
            {
                // no need to validate, just return what we have and assume it's all there is
                return entities.WhereNotNull().ToArray();
            }
        }
        else if (_options.GetAllCacheAllowZeroCount)
        {
            // if none of them were in the cache
            // and we allow zero count - check for the special (empty) entry
            TEntity[]? empty = Cache.GetCacheItem<TEntity[]>(EntityTypeCacheKey);
            if (empty != null)
            {
                return empty;
            }
        }

        // cache failed, get from repo and cache
        IEnumerable<TEntity>? repoResult = await performGetAll();
        TEntity[]? repoEntities = repoResult?
            .WhereNotNull() // exclude nulls!
            .Where(x => x.HasIdentity) // be safe, though would be weird...
            .ToArray();

        InsertEntities(repoEntities);

        return repoEntities ?? Array.Empty<TEntity>();
    }

    /// <inheritdoc />
    public override async Task<TEntity[]> GetManyAsync(TKey[] keys, Func<TKey[], Task<IEnumerable<TEntity>?>> performGetMany, Func<Task<IEnumerable<TEntity>?>> performGetAll)
    {
        await EnsureCacheIsSyncedAsync();
        if (keys.Length > 0)
        {
            // try to get each entity from the cache
            // if we can find all of them, return
            TEntity[] entities = (await Task.WhenAll(keys.Select(GetCachedAsync))).WhereNotNull().ToArray();
            if (keys.Length.Equals(entities.Length))
            {
                return entities; // no need for null checks, we are not caching nulls
            }
        }
        else
        {
            // get everything we have
            TEntity?[] entities = Cache.GetCacheItemsByKeySearch<TEntity>(EntityTypeCacheKey)
                .ToArray(); // no need for null checks, we are not caching nulls

            if (entities.Length > 0)
            {
                // if some of them were in the cache...
                if (_options.GetAllCacheValidateCount)
                {
                    // need to validate the count, get the actual count and return if ok
                    if (_options.PerformCountAsync is not null)
                    {
                        var totalCount = await _options.PerformCountAsync();
                        if (entities.Length == totalCount)
                        {
                            return entities.WhereNotNull().ToArray();
                        }
                    }
                }
                else
                {
                    // no need to validate, just return what we have and assume it's all there is
                    return entities.WhereNotNull().ToArray();
                }
            }
            else if (_options.GetAllCacheAllowZeroCount)
            {
                // if none of them were in the cache
                // and we allow zero count - check for the special (empty) entry
                TEntity[]? empty = Cache.GetCacheItem<TEntity[]>(EntityTypeCacheKey);
                if (empty != null)
                {
                    return empty;
                }
            }
        }

        // cache failed, get from repo and cache
        IEnumerable<TEntity>? repoResult = await performGetMany(keys);
        TEntity[]? repoEntities = repoResult?
            .WhereNotNull() // exclude nulls!
            .Where(x => x.HasIdentity) // be safe, though would be weird...
            .ToArray();

        // note: if empty & allow zero count, will cache a special (empty) entry
        InsertEntities(repoEntities);

        return repoEntities ?? Array.Empty<TEntity>();
    }

    /// <inheritdoc />
    public override async Task ClearAllAsync() => Cache.ClearByKey(EntityTypeCacheKey);

    /// <summary>
    ///     Gets the cache key for an entity with the specified integer identifier.
    /// </summary>
    /// <param name="id">The integer identifier.</param>
    /// <returns>The cache key.</returns>
    protected string GetEntityCacheKey(int id) => EntityTypeCacheKey + id;

    /// <summary>
    ///     Gets the cache key for an entity with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The cache key.</returns>
    protected string GetEntityCacheKey(TKey? key)
    {
        if (EqualityComparer<TKey>.Default.Equals(key, default))
        {
            return string.Empty;
        }

        if (typeof(TKey).IsValueType)
        {
            return EntityTypeCacheKey + key;
        }

        return EntityTypeCacheKey + key?.ToString()?.ToUpperInvariant();
    }

    /// <summary>
    ///     Inserts an entity into the cache with a sliding expiration.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="entity">The entity to cache.</param>
    protected virtual void InsertEntity(string cacheKey, TEntity entity)
        => Cache.Insert(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);

    /// <summary>
    ///     Inserts a sentinel null value into the cache to represent a known missing entity.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    protected virtual void InsertNull(string cacheKey)
    {
        // We can't actually cache a null value, as in doing so wouldn't be able to distinguish between
        // a value that does exist but isn't yet cached, or a value that has been explicitly cached with a null value.
        // Both would return null when we retrieve from the cache and we couldn't distinguish between the two.
        // So we cache a special value that represents null, and then we can check for that value when we retrieve from the cache.
        Cache.Insert(cacheKey, () => Constants.Cache.NullRepresentationInCache, TimeSpan.FromMinutes(5), true);
    }

    /// <summary>
    ///     Inserts a set of entities individually into the cache, each with a sliding expiration.
    /// </summary>
    /// <param name="entities">The entities to cache, or <see langword="null"/> to cache nothing.</param>
    protected virtual void InsertEntities(TEntity[]? entities)
    {
        if (entities is not null)
        {
            // individually cache each item
            foreach (TEntity entity in entities)
            {
                TEntity capture = entity;
                Cache.Insert(GetEntityCacheKey(entity.Id), () => capture, TimeSpan.FromMinutes(5), true);
            }
        }
    }
}
