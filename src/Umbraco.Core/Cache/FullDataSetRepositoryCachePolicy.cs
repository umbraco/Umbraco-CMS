using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Collections;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A caching policy that caches an entire dataset as a single collection
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <remarks>
    /// This caching policy has no sliding expiration but uses the default ObjectCache.InfiniteAbsoluteExpiration as it's timeout, so it
    /// should not leave the cache unless the cache memory is exceeded and it gets thrown out.
    /// </remarks>
    internal class FullDataSetRepositoryCachePolicy<TEntity, TId> : DefaultRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private readonly Func<TEntity, TId> _getEntityId;

        public FullDataSetRepositoryCachePolicy(IRuntimeCacheProvider cache, Func<TEntity, TId> getEntityId) : base(cache,
            new RepositoryCachePolicyOptions
            {
                //Definitely allow zero'd cache entires since this is a full set, in many cases there will be none,
                // and we must cache this!
                GetAllCacheAllowZeroCount = true
            })
        {
            _getEntityId = getEntityId;
        }

        private bool? _hasZeroCountCache;


        public override TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> getFromRepo)
        {
            //process the base logic without any Ids - we want to cache them all!
            var result = base.GetAll(new TId[] { }, getFromRepo);

            //now that the base result has been calculated, they will all be cached. 
            // Now we can just filter by ids if they have been supplied
            
            return ids.Any() 
                ? result.Where(x => ids.Contains(_getEntityId(x))).ToArray() 
                : result;
        }

        /// <summary>
        /// For this type of caching policy, we don't cache individual items
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="entity"></param>
        protected override void SetCacheAction(string cacheKey, TEntity entity)
        {
            //do nothing
        }

        /// <summary>
        /// Sets the action to execute on disposal for an entity collection
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="entityCollection"></param>
        protected override void SetCacheAction(TId[] ids, TEntity[] entityCollection)
        {
            //for this type of caching policy, we don't want to cache any GetAll request containing specific Ids
            if (ids.Any()) return;

            //set the disposal action
            SetCacheAction(() =>
            {
                //We want to cache the result as a single collection
                Cache.InsertCacheItem(GetCacheTypeKey(), () => new DeepCloneableList<TEntity>(entityCollection));
            });
        }

        /// <summary>
        /// Looks up the zero count cache, must return null if it doesn't exist
        /// </summary>
        /// <returns></returns>
        protected override bool HasZeroCountCache()
        {
            if (_hasZeroCountCache.HasValue)
                return _hasZeroCountCache.Value;

            _hasZeroCountCache = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetCacheTypeKey()) != null;
            return _hasZeroCountCache.Value;
        }

        /// <summary>
        /// This policy will cache the full data set as a single collection
        /// </summary>
        /// <returns></returns>
        protected override TEntity[] GetAllFromCache()
        {
            var found = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetCacheTypeKey());
            
            //This method will get called before checking for zero count cache, so we'll just set the flag here
            _hasZeroCountCache = found != null;

            return found == null ? new TEntity[] { } : found.WhereNotNull().ToArray();
        }
    }
}