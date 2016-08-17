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
    internal abstract class RepositoryBase
    {
        protected RepositoryBase(IUnitOfWork work, CacheHelper cache, ILogger logger)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            Logger = logger;
            UnitOfWork = work;
            RepositoryCache = cache;
        }

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
        protected internal IUnitOfWork UnitOfWork { get; }

        protected CacheHelper RepositoryCache { get; }

        /// <summary>
        /// The runtime cache used for this repo - by standard this is the runtime cache exposed by the CacheHelper but can be overridden
        /// </summary>
        protected virtual IRuntimeCacheProvider RuntimeCache => RepositoryCache.RuntimeCache;

        public static string GetCacheIdKey<T>(object id)
        {
            return $"{GetCacheTypeKey<T>()}{id}";
        }

        public static string GetCacheTypeKey<T>()
        {
            return $"uRepo_{typeof (T).Name}_";
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

        /// <summary>
        /// Used to create a new query instance
        /// </summary>
        /// <returns></returns>
        public abstract IQuery<TEntity> Query { get; }

        /// <summary>
        /// Returns a query factory instance
        /// </summary>
        public abstract IQueryFactory QueryFactory { get; }

        protected virtual TId GetEntityId(TEntity entity)
        {
            return (TId)(object)entity.Id;
        }

        /// <summary>
        /// The runtime cache used for this repo by default is the isolated cache for this type
        /// </summary>
        protected override IRuntimeCacheProvider RuntimeCache => RepositoryCache.IsolatedRuntimeCache.GetOrCreateCache<TEntity>();

        private IRepositoryCachePolicy<TEntity, TId> _cachePolicy;

        protected virtual IRepositoryCachePolicy<TEntity, TId> CachePolicy
        {
            get
            {
                if (_cachePolicy != null) return _cachePolicy;

                var options = new RepositoryCachePolicyOptions(() =>
                {
                    //Get count of all entities of current type (TEntity) to ensure cached result is correct
                    var query = Query.Where(x => x.Id != 0);
                    return PerformCount(query);
                });

                _cachePolicy = new DefaultRepositoryCachePolicy<TEntity, TId>(RuntimeCache, options);

                return _cachePolicy;
            }
        }

        /// <summary>
        /// Adds or Updates an entity of type TEntity
        /// </summary>
        /// <remarks>This method is backed by an <see cref="IRuntimeCacheProvider"/> cache</remarks>
        /// <param name="entity"></param>
        public void AddOrUpdate(TEntity entity)
        {
            if (entity.HasIdentity == false)
            {
                UnitOfWork.RegisterCreated(entity, this);
            }
            else
            {
                UnitOfWork.RegisterUpdated(entity, this);
            }
        }

        /// <summary>
        /// Deletes the passed in entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            UnitOfWork?.RegisterDeleted(entity, this);
        }

        protected abstract TEntity PerformGet(TId id);
        /// <summary>
        /// Gets an entity by the passed in Id utilizing the repository's cache policy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(TId id)
        {
            return CachePolicy.Get(id, PerformGet, PerformGetAll);
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

            return CachePolicy.GetAll(ids, PerformGetAll);
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
            return CachePolicy.Exists(id, PerformExists, PerformGetAll);
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
        
        /// <summary>
        /// Unit of work method that tells the repository to persist the new entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistNewItem(IEntity entity)
        {
            CachePolicy.Create((TEntity) entity, PersistNewItem);
        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the updated entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistUpdatedItem(IEntity entity)
        {
            CachePolicy.Update((TEntity) entity, PersistUpdatedItem);
        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the deletion of the entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistDeletedItem(IEntity entity)
        {
            CachePolicy.Delete((TEntity) entity, PersistDeletedItem);
        }
        

        protected abstract void PersistNewItem(TEntity item);
        protected abstract void PersistUpdatedItem(TEntity item);
        protected abstract void PersistDeletedItem(TEntity item);
    }
}