using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Represents the default cache policy.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    /// <remarks>
    /// <para>The default cache policy caches entities with a 5 minutes sliding expiration.</para>
    /// <para>Each entity is cached individually.</para>
    /// <para>If options.GetAllCacheAllowZeroCount then a 'zero-count' array is cached when GetAll finds nothing.</para>
    /// <para>If options.GetAllCacheValidateCount then we check against the db when getting many entities.</para>
    /// </remarks>
    internal class DefaultRepositoryCachePolicy<TEntity, TId> : RepositoryCachePolicyBase<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private static readonly TEntity[] EmptyEntities = new TEntity[0];
        private readonly RepositoryCachePolicyOptions _options;

        public DefaultRepositoryCachePolicy(IRuntimeCacheProvider cache, RepositoryCachePolicyOptions options)
            : base(cache)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options;
        }

        protected string GetEntityCacheKey(object id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return GetEntityTypeCacheKey() + id;
        }

        protected string GetEntityTypeCacheKey()
        {
            return $"uRepo_{typeof (TEntity).Name}_";
        }

        /// <summary>
        /// Sets the action to execute on disposal for a single entity
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="entity"></param>
        protected virtual void SetCacheActionToInsertEntity(string cacheKey, TEntity entity)
        {
            SetCacheAction(() =>
            {
                Cache.InsertCacheItem(cacheKey, () => entity, TimeSpan.FromMinutes(5), true);
            });
        }

        /// <summary>
        /// Sets the action to execute on disposal for an entity collection
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="entities"></param>
        protected virtual void SetCacheActionToInsertEntities(TId[] ids, TEntity[] entities)
        {
            SetCacheAction(() =>
            {
                if (ids.Length == 0 && entities.Length == 0 && _options.GetAllCacheAllowZeroCount)
                {
                    // getting all of them, and finding nothing.
                    // if we can cache a zero count, cache an empty array,
                    // for as long as the cache is not cleared (no expiration)
                    Cache.InsertCacheItem(GetEntityTypeCacheKey(), () => EmptyEntities);
                }
                else
                {
                    // individually cache each item
                    foreach (var entity in entities)
                    {
                        var capture = entity;
                        Cache.InsertCacheItem(GetEntityCacheKey(entity.Id), () => capture, TimeSpan.FromMinutes(5), true);
                    }
                }
            });
        }

        /// <inheritdoc />
        public override void CreateOrUpdate(TEntity entity, Action<TEntity> repoCreateOrUpdate)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (repoCreateOrUpdate == null) throw new ArgumentNullException(nameof(repoCreateOrUpdate));

            try
            {
                repoCreateOrUpdate(entity);

                SetCacheAction(() =>
                {
                    // just to be safe, we cannot cache an item without an identity
                    if (entity.HasIdentity)
                    {
                        Cache.InsertCacheItem(GetEntityCacheKey(entity.Id), () => entity, TimeSpan.FromMinutes(5), true);
                    }

                    // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                    Cache.ClearCacheItem(GetEntityTypeCacheKey());
                });

            }
            catch
            {
                SetCacheAction(() =>
                {
                    // if an exception is thrown we need to remove the entry from cache,
                    // this is ONLY a work around because of the way
                    // that we cache entities: http://issues.umbraco.org/issue/U4-4259
                    Cache.ClearCacheItem(GetEntityCacheKey(entity.Id));

                    // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                    Cache.ClearCacheItem(GetEntityTypeCacheKey());
                });

                throw;
            }
        }

        /// <inheritdoc />
        public override void Remove(TEntity entity, Action<TEntity> repoRemove)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (repoRemove == null) throw new ArgumentNullException(nameof(repoRemove));

            try
            {
                repoRemove(entity);
            }
            finally
            {
                // whatever happens, clear the cache
                var cacheKey = GetEntityCacheKey(entity.Id);
                SetCacheAction(() =>
                {
                    Cache.ClearCacheItem(cacheKey);

                    // if there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                    Cache.ClearCacheItem(GetEntityTypeCacheKey());
                });
            }
        }

        /// <inheritdoc />
        public override TEntity Get(TId id, Func<TId, TEntity> repoGet)
        {
            if (repoGet == null) throw new ArgumentNullException(nameof(repoGet));

            var cacheKey = GetEntityCacheKey(id);
            var fromCache = Cache.GetCacheItem<TEntity>(cacheKey);

            // if found in cache then return else fetch and cache
            if (fromCache != null)
                return fromCache;
            var entity = repoGet(id);

            if (entity != null && entity.HasIdentity)
                SetCacheActionToInsertEntity(cacheKey, entity);

            return entity;
        }

        /// <inheritdoc />
        public override TEntity Get(TId id)
        {
            var cacheKey = GetEntityCacheKey(id);
            return Cache.GetCacheItem<TEntity>(cacheKey);
        }

        /// <inheritdoc />
        public override bool Exists(TId id, Func<TId, bool> repoExists)
        {
            if (repoExists == null) throw new ArgumentNullException(nameof(repoExists));

            // if found in cache the return else check
            var cacheKey = GetEntityCacheKey(id);
            var fromCache = Cache.GetCacheItem<TEntity>(cacheKey);
            return fromCache != null || repoExists(id);
        }

        /// <inheritdoc />
        public override TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> repoGet)
        {
            if (repoGet == null) throw new ArgumentNullException(nameof(repoGet));

            if (ids.Length > 0)
            {
                // try to get each entity from the cache
                // if we can find all of them, return
                var entities = ids.Select(Get).ToArray();
                if (ids.Length.Equals(entities.Length))
                    return entities; // no need for null checks, we are not caching nulls
            }
            else
            {
                // get everything we have
                var entities = Cache.GetCacheItemsByKeySearch<TEntity>(GetEntityTypeCacheKey())
                    .ToArray(); // no need for null checks, we are not caching nulls

                if (entities.Length > 0)
                {
                    // if some of them were in the cache...
                    if (_options.GetAllCacheValidateCount)
                    {
                        // need to validate the count, get the actual count and return if ok
                        var totalCount = _options.PerformCount();
                        if (entities.Length == totalCount)
                            return entities;
                    }
                    else
                    {
                        // no need to validate, just return what we have and assume it's all there is
                        return entities;
                    }
                }
                else if (_options.GetAllCacheAllowZeroCount)
                {
                    // if none of them were in the cache
                    // and we allow zero count - check for the special (empty) entry
                    var empty = Cache.GetCacheItem<TEntity[]>(GetEntityTypeCacheKey());
                    if (empty != null) return empty;
                }
            }

            // cache failed, get from repo and cache
            var repoEntities = repoGet(ids)
                .WhereNotNull() // exclude nulls!
                .Where(x => x.HasIdentity) // be safe, though would be weird...
                .ToArray();

            // note: if empty & allow zero count, will cache a special (empty) entry
            SetCacheActionToInsertEntities(ids, repoEntities);

            return repoEntities;
        }
    }
}