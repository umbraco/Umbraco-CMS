using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;

using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class RepositoryBase : DisposableObject
    {
        private readonly IUnitOfWork _work;
        private readonly CacheHelper _cache;

        protected RepositoryBase(IUnitOfWork work, CacheHelper cache, ILogger logger)
        {
            if (work == null) throw new ArgumentNullException("work");
            if (cache == null) throw new ArgumentNullException("cache");
            if (logger == null) throw new ArgumentNullException("logger");
            Logger = logger;
            _work = work;

            //IMPORTANT: We will force the DeepCloneRuntimeCacheProvider to be used here which is a wrapper for the underlying
            // runtime cache to ensure that anything that can be deep cloned in/out is done so, this also ensures that our tracks
            // changes entities are reset.
            _cache = new CacheHelper(new DeepCloneRuntimeCacheProvider(cache.RuntimeCache), cache.StaticCache, cache.RequestCache);            
        }

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
        protected internal IUnitOfWork UnitOfWork
        {
            get { return _work; }
        }

        /// <summary>
        /// Internal for testing purposes
        /// </summary>
        internal Guid UnitKey
        {
            get { return (Guid)_work.Key; }
        }

        protected CacheHelper RepositoryCache
        {
            get { return _cache; }
        }

        /// <summary>
        /// The runtime cache used for this repo - by standard this is the runtime cache exposed by the CacheHelper but can be overridden
        /// </summary>
        protected virtual IRuntimeCacheProvider RuntimeCache
        {
            get { return _cache.RuntimeCache; }
        }

        public static string GetCacheIdKey<T>(object id)
        {
            return string.Format("{0}{1}", GetCacheTypeKey<T>(), id);
        }

        public static string GetCacheTypeKey<T>()
        {
            return string.Format("uRepo_{0}_", typeof(T).Name);
        }

        protected ILogger Logger { get; private set; }
    }

    /// <summary>
    /// Represent an abstract Repository, which is the base of the Repository implementations
    /// </summary>
    /// <typeparam name="TEntity">Type of <see cref="IAggregateRoot"/> entity for which the repository is used</typeparam>
    /// <typeparam name="TId">Type of the Id used for this entity</typeparam>
    internal abstract class RepositoryBase<TId, TEntity> : RepositoryBase, IRepositoryQueryable<TId, TEntity>, IUnitOfWorkRepository
        where TEntity : class, IAggregateRoot
    {
        protected RepositoryBase(IUnitOfWork work, CacheHelper cache, ILogger logger)
            : base(work, cache, logger)
        {
        }

        private readonly RepositoryCacheOptions _cacheOptions = new RepositoryCacheOptions();

        #region IRepository<TEntity> Members

        /// <summary>
        /// Adds or Updates an entity of type TEntity
        /// </summary>
        /// <remarks>This method is backed by an <see cref="IRuntimeCacheProvider"/> cache</remarks>
        /// <param name="entity"></param>
        public void AddOrUpdate(TEntity entity)
        {
            if (entity.HasIdentity == false)
            {
                UnitOfWork.RegisterAdded(entity, this);
            }
            else
            {
                UnitOfWork.RegisterChanged(entity, this);
            }
        }

        /// <summary>
        /// Deletes the passed in entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            if (UnitOfWork != null)
            {
                UnitOfWork.RegisterRemoved(entity, this);
            }
        }

        protected abstract TEntity PerformGet(TId id);
        /// <summary>
        /// Gets an entity by the passed in Id utilizing the repository's runtime cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(TId id)
        {
            var cacheKey = GetCacheIdKey<TEntity>(id);
            var fromCache = RuntimeCache.GetCacheItem<TEntity>(cacheKey);

            if (fromCache != null) return fromCache;

            var entity = PerformGet(id);
            if (entity == null) return null;
            
            RuntimeCache.InsertCacheItem(cacheKey, () => entity);

            return entity;
        }

        protected abstract IEnumerable<TEntity> PerformGetAll(params TId[] ids);
        /// <summary>
        /// Gets all entities of type TEntity or a list according to the passed in Ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetAll(params TId[] ids)
        {
            //ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
            ids = ids.Distinct()
                //don't query by anything that is a default of T (like a zero)
                //TODO: I think we should enabled this in case accidental calls are made to get all with invalid ids
                //.Where(x => Equals(x, default(TId)) == false)
                .ToArray();

            if (ids.Length > 2000)
            {
                throw new InvalidOperationException("Cannot perform a query with more than 2000 parameters");
            }

            if (ids.Any())
            {
                var entities = ids.Select(x => RuntimeCache.GetCacheItem<TEntity>(GetCacheIdKey<TEntity>(x))).ToArray();

                if (ids.Count().Equals(entities.Count()) && entities.Any(x => x == null) == false)
                    return entities;
            }
            else
            {
                var allEntities = RuntimeCache.GetCacheItemsByKeySearch<TEntity>(GetCacheTypeKey<TEntity>())
                    .WhereNotNull()
                    .ToArray();

                if (allEntities.Any())
                {

                    if (RepositoryCacheOptions.GetAllCacheValidateCount)
                    {
                        //Get count of all entities of current type (TEntity) to ensure cached result is correct
                        var query = Query<TEntity>.Builder.Where(x => x.Id != 0);
                        int totalCount = PerformCount(query);

                        if (allEntities.Count() == totalCount)
                            return allEntities;
                    }
                    else
                    {
                        return allEntities;
                    }
                }
                else if (RepositoryCacheOptions.GetAllCacheAllowZeroCount)
                {
                    //if the repository allows caching a zero count, then check the zero count cache
                    var zeroCount = RuntimeCache.GetCacheItem<TEntity[]>(GetCacheTypeKey<TEntity>());
                    if (zeroCount != null && zeroCount.Any() == false)
                    {
                        //there is a zero count cache so return an empty list
                        return Enumerable.Empty<TEntity>();
                    }
                }

            }

            var entityCollection = PerformGetAll(ids)
                //ensure we don't include any null refs in the returned collection!
                .WhereNotNull()
                .ToArray();

            //We need to put a threshold here! IF there's an insane amount of items
            // coming back here we don't want to chuck it all into memory, this added cache here
            // is more for convenience when paging stuff temporarily

            if (entityCollection.Length > RepositoryCacheOptions.GetAllCacheThresholdLimit) 
                return entityCollection;

            if (entityCollection.Length == 0 && RepositoryCacheOptions.GetAllCacheAllowZeroCount)
            {
                //there was nothing returned but we want to cache a zero count result so add an TEntity[] to the cache
                // to signify that there is a zero count cache
                RuntimeCache.InsertCacheItem(GetCacheTypeKey<TEntity>(), () => new TEntity[] {});
            }

            foreach (var entity in entityCollection)
            {
                if (entity != null)
                {
                    var localCopy = entity;
                    RuntimeCache.InsertCacheItem(GetCacheIdKey<TEntity>(entity.Id), () => localCopy);
                }
            }

            return entityCollection;
        }

        /// <summary>
        /// Returns the repository cache options
        /// </summary>
        protected virtual RepositoryCacheOptions RepositoryCacheOptions
        {
            get { return _cacheOptions; }
        }

        protected abstract IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query);
        /// <summary>
        /// Gets a list of entities by the passed in query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetByQuery(IQuery<TEntity> query)
        {
            return PerformGetByQuery(query)
                //ensure we don't include any null refs in the returned collection!
                .WhereNotNull();
        }

        protected abstract bool PerformExists(TId id);
        /// <summary>
        /// Returns a boolean indicating whether an entity with the passed Id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(TId id)
        {
            var fromCache = RuntimeCache.GetCacheItem<TEntity>(GetCacheIdKey<TEntity>(id));
            if (fromCache != null)
            {
                return true;
            }
            return PerformExists(id);
        }

        protected abstract int PerformCount(IQuery<TEntity> query);
        /// <summary>
        /// Returns an integer with the count of entities found with the passed in query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public int Count(IQuery<TEntity> query)
        {
            return PerformCount(query);
        }

        #endregion

        #region IUnitOfWorkRepository Members

        /// <summary>
        /// Unit of work method that tells the repository to persist the new entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistNewItem(IEntity entity)
        {
            try
            {
                PersistNewItem((TEntity)entity);
                RuntimeCache.InsertCacheItem(GetCacheIdKey<TEntity>(entity.Id), () => entity);
                //If there's a GetAll zero count cache, ensure it is cleared
                RuntimeCache.ClearCacheItem(GetCacheTypeKey<TEntity>());
            }
            catch (Exception)
            {
                //if an exception is thrown we need to remove the entry from cache, this is ONLY a work around because of the way
                // that we cache entities: http://issues.umbraco.org/issue/U4-4259
                RuntimeCache.ClearCacheItem(GetCacheIdKey<TEntity>(entity.Id));
                //If there's a GetAll zero count cache, ensure it is cleared
                RuntimeCache.ClearCacheItem(GetCacheTypeKey<TEntity>());
                throw;
            }

        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the updated entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistUpdatedItem(IEntity entity)
        {
            try
            {
                PersistUpdatedItem((TEntity)entity);
                RuntimeCache.InsertCacheItem(GetCacheIdKey<TEntity>(entity.Id), () => entity);
                //If there's a GetAll zero count cache, ensure it is cleared
                RuntimeCache.ClearCacheItem(GetCacheTypeKey<TEntity>());
            }
            catch (Exception)
            {
                //if an exception is thrown we need to remove the entry from cache, this is ONLY a work around because of the way
                // that we cache entities: http://issues.umbraco.org/issue/U4-4259
                RuntimeCache.ClearCacheItem(GetCacheIdKey<TEntity>(entity.Id));
                //If there's a GetAll zero count cache, ensure it is cleared
                RuntimeCache.ClearCacheItem(GetCacheTypeKey<TEntity>());
                throw;
            }

        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the deletion of the entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistDeletedItem(IEntity entity)
        {
            PersistDeletedItem((TEntity)entity);
            RuntimeCache.ClearCacheItem(GetCacheIdKey<TEntity>(entity.Id));
            //If there's a GetAll zero count cache, ensure it is cleared
            RuntimeCache.ClearCacheItem(GetCacheTypeKey<TEntity>());
        }

        #endregion

        #region Abstract IUnitOfWorkRepository Methods

        protected abstract void PersistNewItem(TEntity item);
        protected abstract void PersistUpdatedItem(TEntity item);
        protected abstract void PersistDeletedItem(TEntity item);

        #endregion

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            UnitOfWork.DisposeIfDisposable();
        }
    }
}