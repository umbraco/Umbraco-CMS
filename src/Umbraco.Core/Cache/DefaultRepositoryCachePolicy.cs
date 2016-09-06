using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// The default cache policy for retrieving a single entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <remarks>
    /// This cache policy uses sliding expiration and caches instances for 5 minutes. However if allow zero count is true, then we use the
    /// default policy with no expiry.
    /// </remarks>
    internal class DefaultRepositoryCachePolicy<TEntity, TId> : RepositoryCachePolicyBase<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private readonly RepositoryCachePolicyOptions _options;
       
        public DefaultRepositoryCachePolicy(IRuntimeCacheProvider cache, RepositoryCachePolicyOptions options)
            : base(cache)
        {            
            if (options == null) throw new ArgumentNullException("options");
            _options = options;         
        }

        protected string GetCacheIdKey(object id)
        {
            if (id == null) throw new ArgumentNullException("id");

            return string.Format("{0}{1}", GetCacheTypeKey(), id);
        }

        protected string GetCacheTypeKey()
        {
            return string.Format("uRepo_{0}_", typeof(TEntity).Name);
        }

        public override void CreateOrUpdate(TEntity entity, Action<TEntity> persistMethod)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (persistMethod == null) throw new ArgumentNullException("persistMethod");

            try
            {
                persistMethod(entity);

                //set the disposal action                
                SetCacheAction(() =>
                {
                    //just to be safe, we cannot cache an item without an identity
                    if (entity.HasIdentity)
                    {
                        Cache.InsertCacheItem(GetCacheIdKey(entity.Id), () => entity,
                            timeout: TimeSpan.FromMinutes(5),
                            isSliding: true);
                    }
                    
                    //If there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                    Cache.ClearCacheItem(GetCacheTypeKey());
                });
                
            }
            catch
            {
                //set the disposal action                
                SetCacheAction(() =>
                {
                    //if an exception is thrown we need to remove the entry from cache, this is ONLY a work around because of the way
                    // that we cache entities: http://issues.umbraco.org/issue/U4-4259
                    Cache.ClearCacheItem(GetCacheIdKey(entity.Id));

                    //If there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                    Cache.ClearCacheItem(GetCacheTypeKey());
                });
                
                throw;
            }
        }

        public override void Remove(TEntity entity, Action<TEntity> persistMethod)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (persistMethod == null) throw new ArgumentNullException("persistMethod");

            try
            {
                persistMethod(entity);
            }
            finally
            {
                //set the disposal action
                var cacheKey = GetCacheIdKey(entity.Id);
                SetCacheAction(() =>
                {
                    Cache.ClearCacheItem(cacheKey);
                    //If there's a GetAllCacheAllowZeroCount cache, ensure it is cleared
                    Cache.ClearCacheItem(GetCacheTypeKey());
                });
            }
        }

        public override TEntity Get(TId id, Func<TId, TEntity> getFromRepo)
        {
            if (getFromRepo == null) throw new ArgumentNullException("getFromRepo");

            var cacheKey = GetCacheIdKey(id);
            var fromCache = Cache.GetCacheItem<TEntity>(cacheKey);
            if (fromCache != null)
                return fromCache;
            
            var entity = getFromRepo(id);

            //set the disposal action
            SetCacheAction(cacheKey, entity);

            return entity;
        }

        public override TEntity Get(TId id)
        {
            var cacheKey = GetCacheIdKey(id);
            return Cache.GetCacheItem<TEntity>(cacheKey);
        }

        public override bool Exists(TId id, Func<TId, bool> getFromRepo)
        {
            if (getFromRepo == null) throw new ArgumentNullException("getFromRepo");

            var cacheKey = GetCacheIdKey(id);
            var fromCache = Cache.GetCacheItem<TEntity>(cacheKey);
            return fromCache != null || getFromRepo(id);
        }

        public override TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> getFromRepo)            
        {
            if (getFromRepo == null) throw new ArgumentNullException("getFromRepo");

            if (ids.Any())
            {
                var entities = ids.Select(Get).ToArray();
                if (ids.Length.Equals(entities.Length) && entities.Any(x => x == null) == false)
                    return entities;
            }
            else
            {
                var allEntities = GetAllFromCache();
                if (allEntities.Any())
                {
                    if (_options.GetAllCacheValidateCount)
                    {
                        //Get count of all entities of current type (TEntity) to ensure cached result is correct
                        var totalCount = _options.PerformCount();
                        if (allEntities.Length == totalCount)
                            return allEntities;
                    }
                    else
                    {
                        return allEntities;
                    }
                }
                else if (_options.GetAllCacheAllowZeroCount)
                {
                    //if the repository allows caching a zero count, then check the zero count cache
                    if (HasZeroCountCache())
                    {
                        //there is a zero count cache so return an empty list
                        return new TEntity[] {};
                    }
                }
            }

            //we need to do the lookup from the repo
            var entityCollection = getFromRepo(ids)
                //ensure we don't include any null refs in the returned collection!
                .WhereNotNull()
                .ToArray();

            //set the disposal action
            SetCacheAction(ids, entityCollection);

            return entityCollection;
        }

        /// <summary>
        /// Looks up the zero count cache, must return null if it doesn't exist
        /// </summary>
        /// <returns></returns>
        protected bool HasZeroCountCache()
        {
            var zeroCount = Cache.GetCacheItem<TEntity[]>(GetCacheTypeKey());
            return (zeroCount != null && zeroCount.Any() == false);
        }

        /// <summary>
        /// Performs the lookup for all entities of this type from the cache
        /// </summary>
        /// <returns></returns>
        protected TEntity[] GetAllFromCache()
        {
            var allEntities = Cache.GetCacheItemsByKeySearch<TEntity>(GetCacheTypeKey())
                    .WhereNotNull()
                    .ToArray();
            return allEntities.Any() ? allEntities : new TEntity[] {};
        }       

        /// <summary>
        /// Sets the action to execute on disposal for a single entity
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="entity"></param>
        protected virtual void SetCacheAction(string cacheKey, TEntity entity)
        {
            if (entity == null) return;

            SetCacheAction(() =>
            {
                //just to be safe, we cannot cache an item without an identity
                if (entity.HasIdentity)
                {
                    Cache.InsertCacheItem(cacheKey, () => entity,
                        timeout: TimeSpan.FromMinutes(5),
                        isSliding: true);
                }
            });
        }

        /// <summary>
        /// Sets the action to execute on disposal for an entity collection
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="entityCollection"></param>
        protected virtual void SetCacheAction(TId[] ids, TEntity[] entityCollection)
        {
            SetCacheAction(() =>
            {
                //This option cannot execute if we are looking up specific Ids
                if (ids.Any() == false && entityCollection.Length == 0 && _options.GetAllCacheAllowZeroCount)
                {
                    //there was nothing returned but we want to cache a zero count result so add an TEntity[] to the cache
                    // to signify that there is a zero count cache
                    //NOTE: Don't set expiry/sliding for a zero count
                    Cache.InsertCacheItem(GetCacheTypeKey(), () => new TEntity[] {});
                }
                else
                {
                    //This is the default behavior, we'll individually cache each item so that if/when these items are resolved 
                    // by id, they are returned from the already existing cache.
                    foreach (var entity in entityCollection.WhereNotNull())
                    {
                        var localCopy = entity;
                        //just to be safe, we cannot cache an item without an identity
                        if (localCopy.HasIdentity)
                        {
                            Cache.InsertCacheItem(GetCacheIdKey(entity.Id), () => localCopy,
                                timeout: TimeSpan.FromMinutes(5),
                                isSliding: true);
                        }
                    }
                }
            });
        }
        
    }
}