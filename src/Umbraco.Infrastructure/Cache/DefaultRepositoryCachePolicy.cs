// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents the default cache policy.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <remarks>
///     <para>The default cache policy caches entities with a 5 minutes sliding expiration.</para>
///     <para>Each entity is cached individually.</para>
///     <para>If options.GetAllCacheAllowZeroCount then a 'zero-count' array is cached when GetAll finds nothing.</para>
///     <para>If options.GetAllCacheValidateCount then we check against the db when getting many entities.</para>
/// </remarks>
public class DefaultRepositoryCachePolicy<TEntity, TId> : RepositoryCachePolicyBase<TEntity, TId>
    where TEntity : class, IEntity
{
    private static readonly TEntity[] _emptyEntities = new TEntity[0]; // const
    private readonly RepositoryCachePolicyOptions _options;

    public DefaultRepositoryCachePolicy(IAppPolicyCache cache, IScopeAccessor scopeAccessor, RepositoryCachePolicyOptions options)
        : base(cache, scopeAccessor) =>
        _options = options ?? throw new ArgumentNullException(nameof(options));

    protected string EntityTypeCacheKey { get; } = $"uRepo_{typeof(TEntity).Name}_";

    /// <inheritdoc />
    public override void Create(TEntity entity, Action<TEntity> persistNew)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            persistNew(entity);

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
    public override void Update(TEntity entity, Action<TEntity> persistUpdated)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            persistUpdated(entity);

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
    public override void Delete(TEntity entity, Action<TEntity> persistDeleted)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            persistDeleted(entity);
        }
        finally
        {
            // whatever happens, clear the cache
            var cacheKey = GetEntityCacheKey(entity.Id);
            Cache.Clear(cacheKey);

            // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
            Cache.Clear(EntityTypeCacheKey);
        }
    }

    /// <inheritdoc />
    public override TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
    {
        var cacheKey = GetEntityCacheKey(id);
        TEntity? fromCache = Cache.GetCacheItem<TEntity>(cacheKey);

        // if found in cache then return else fetch and cache
        if (fromCache != null)
        {
            return fromCache;
        }

        TEntity? entity = performGet(id);

        if (entity != null && entity.HasIdentity)
        {
            InsertEntity(cacheKey, entity);
        }

        return entity;
    }

    /// <inheritdoc />
    public override TEntity? GetCached(TId id)
    {
        var cacheKey = GetEntityCacheKey(id);
        return Cache.GetCacheItem<TEntity>(cacheKey);
    }

    /// <inheritdoc />
    public override bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>?> performGetAll)
    {
        // if found in cache the return else check
        var cacheKey = GetEntityCacheKey(id);
        TEntity? fromCache = Cache.GetCacheItem<TEntity>(cacheKey);
        return fromCache != null || performExists(id);
    }

    /// <inheritdoc />
    public override TEntity[] GetAll(TId[]? ids, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
    {
        if (ids?.Length > 0)
        {
            // try to get each entity from the cache
            // if we can find all of them, return
            TEntity[] entities = ids.Select(GetCached).WhereNotNull().ToArray();
            if (ids.Length.Equals(entities.Length))
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
                    if (_options.PerformCount is not null)
                    {
                        var totalCount = _options.PerformCount();
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
        TEntity[]? repoEntities = performGetAll(ids)?
            .WhereNotNull() // exclude nulls!
            .Where(x => x.HasIdentity) // be safe, though would be weird...
            .ToArray();

        // note: if empty & allow zero count, will cache a special (empty) entry
        InsertEntities(ids, repoEntities);

        return repoEntities ?? Array.Empty<TEntity>();
    }

    /// <inheritdoc />
    public override void ClearAll() => Cache.ClearByKey(EntityTypeCacheKey);

    protected string GetEntityCacheKey(int id) => EntityTypeCacheKey + id;

    protected string GetEntityCacheKey(TId? id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default))
        {
            return string.Empty;
        }

        if (typeof(TId).IsValueType)
        {
            return EntityTypeCacheKey + id;
        }

        return EntityTypeCacheKey + id?.ToString()?.ToUpperInvariant();
    }

    protected virtual void InsertEntity(string cacheKey, TEntity entity)
        => Cache.Insert(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);

    protected virtual void InsertEntities(TId[]? ids, TEntity[]? entities)
    {
        if (ids?.Length == 0 && entities?.Length == 0 && _options.GetAllCacheAllowZeroCount)
        {
            // getting all of them, and finding nothing.
            // if we can cache a zero count, cache an empty array,
            // for as long as the cache is not cleared (no expiration)
            Cache.Insert(EntityTypeCacheKey, () => _emptyEntities);
        }
        else
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
}
