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
    internal class FullDataSetRepositoryCachePolicy<TEntity, TId> : RepositoryCachePolicyBase<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private readonly Func<TEntity, TId> _getEntityId;
        private readonly Func<IEnumerable<TEntity>> _getAllFromRepo;
        private readonly bool _expires;

        public FullDataSetRepositoryCachePolicy(IRuntimeCacheProvider cache, Func<TEntity, TId> getEntityId, Func<IEnumerable<TEntity>> getAllFromRepo, bool expires)
            : base(cache)
        {
            _getEntityId = getEntityId;
            _getAllFromRepo = getAllFromRepo;
            _expires = expires;
        }

        private bool? _hasZeroCountCache;


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
                    //Clear all
                    Cache.ClearCacheItem(GetCacheTypeKey());
                });
            }
            catch
            {
                //set the disposal action                
                SetCacheAction(() =>
                {
                    //Clear all
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
                SetCacheAction(() =>
                {
                    //Clear all
                    Cache.ClearCacheItem(GetCacheTypeKey());
                });
            }
        }

        public override TEntity Get(TId id, Func<TId, TEntity> getFromRepo)
        {
            //Force get all with cache
            var found = GetAll(new TId[] { }, ids => _getAllFromRepo().WhereNotNull());

            //we don't have anything in cache (this should never happen), just return from the repo
            if (found == null) return getFromRepo(id);
            var entity = found.FirstOrDefault(x => _getEntityId(x).Equals(id));
            if (entity == null) return null;

            //We must ensure to deep clone each one out manually since the deep clone list only clones one way
            return (TEntity)entity.DeepClone();
        }

        public override TEntity Get(TId id)
        {
            //Force get all with cache
            var found = GetAll(new TId[] { }, ids => _getAllFromRepo().WhereNotNull());

            //we don't have anything in cache (this should never happen), just return null
            if (found == null) return null;
            var entity = found.FirstOrDefault(x => _getEntityId(x).Equals(id));
            if (entity == null) return null;

            //We must ensure to deep clone each one out manually since the deep clone list only clones one way
            return (TEntity)entity.DeepClone();
        }

        public override bool Exists(TId id, Func<TId, bool> getFromRepo)
        {
            //Force get all with cache
            var found = GetAll(new TId[] { }, ids => _getAllFromRepo().WhereNotNull());

            //we don't have anything in cache (this should never happen), just return from the repo
            return found == null
                ? getFromRepo(id)
                : found.Any(x => _getEntityId(x).Equals(id));
        }

        public override TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> getFromRepo)
        {
            //process getting all including setting the cache callback
            var result = PerformGetAll(getFromRepo);

            //now that the base result has been calculated, they will all be cached. 
            // Now we can just filter by ids if they have been supplied

            return (ids.Any()
                ? result.Where(x => ids.Contains(_getEntityId(x))).ToArray()
                : result)
                //We must ensure to deep clone each one out manually since the deep clone list only clones one way
                .Select(x => (TEntity)x.DeepClone())
                .ToArray();
        }

        private TEntity[] PerformGetAll(Func<TId[], IEnumerable<TEntity>> getFromRepo)
        {
            var allEntities = GetAllFromCache();
            if (allEntities.Any())
            {
                return allEntities;
            }

            //check the zero count cache
            if (HasZeroCountCache())
            {
                //there is a zero count cache so return an empty list
                return new TEntity[] { };
            }

            //we need to do the lookup from the repo
            var entityCollection = getFromRepo(new TId[] { })
                //ensure we don't include any null refs in the returned collection!
                .WhereNotNull()
                .ToArray();

            //set the disposal action
            SetCacheAction(entityCollection);

            return entityCollection;
        }

        /// <summary>
        /// For this type of caching policy, we don't cache individual items
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="entity"></param>
        protected void SetCacheAction(string cacheKey, TEntity entity)
        {
            //No-op
        }

        /// <summary>
        /// Sets the action to execute on disposal for an entity collection
        /// </summary>
        /// <param name="entityCollection"></param>
        protected void SetCacheAction(TEntity[] entityCollection)
        {
            //set the disposal action
            SetCacheAction(() =>
            {
                //We want to cache the result as a single collection

                if (_expires)
                {
                    Cache.InsertCacheItem(GetCacheTypeKey(), () => new DeepCloneableList<TEntity>(entityCollection),
                        timeout: TimeSpan.FromMinutes(5),
                        isSliding: true);
                }
                else
                {
                    Cache.InsertCacheItem(GetCacheTypeKey(), () => new DeepCloneableList<TEntity>(entityCollection));
                }
            });
        }

        /// <summary>
        /// Looks up the zero count cache, must return null if it doesn't exist
        /// </summary>
        /// <returns></returns>
        protected bool HasZeroCountCache()
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
        protected TEntity[] GetAllFromCache()
        {
            var found = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetCacheTypeKey());

            //This method will get called before checking for zero count cache, so we'll just set the flag here
            _hasZeroCountCache = found != null;

            return found == null ? new TEntity[] { } : found.WhereNotNull().ToArray();
        }

    }
}