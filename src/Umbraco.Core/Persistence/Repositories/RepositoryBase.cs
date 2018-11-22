using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories
{
    internal abstract class RepositoryBase : DisposableObjectSlim
    {
        private readonly IScopeUnitOfWork _work;
        private readonly CacheHelper _globalCache;

        protected RepositoryBase(IScopeUnitOfWork work, CacheHelper cache, ILogger logger)
        {
            if (work == null) throw new ArgumentNullException("work");
            if (cache == null) throw new ArgumentNullException("cache");
            if (logger == null) throw new ArgumentNullException("logger");
            Logger = logger;
            _work = work;
            _globalCache = cache;
        }

        /// <summary>
        /// Returns the Unit of Work added to the repository
        /// </summary>
        protected internal IScopeUnitOfWork UnitOfWork
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

        /// <summary>
        /// Gets the global application cache.
        /// </summary>
        protected CacheHelper GlobalCache
        {
            get { return _globalCache; }
        }

        /// <summary>
        /// Gets the repository isolated cache.
        /// </summary>
        protected abstract IRuntimeCacheProvider IsolatedCache { get; }

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
        protected RepositoryBase(IScopeUnitOfWork work, CacheHelper cache, ILogger logger)
            : base(work, cache, logger)
        {
        }

        #region Static Queries

        private static IQuery<TEntity> _hasIdQuery;

        #endregion

        protected virtual TId GetEntityId(TEntity entity)
        {
            return (TId)(object)entity.Id;
        }

        /// <summary>
        /// The runtime cache used for this repo by default is the isolated cache for this type
        /// </summary>
        private IRuntimeCacheProvider _isolatedCache;
        protected override IRuntimeCacheProvider IsolatedCache
        {
            get
            {
                if (_isolatedCache != null) return _isolatedCache;

                var scope = UnitOfWork.Scope;
                IsolatedRuntimeCache provider;
                switch (scope.RepositoryCacheMode)
                {
                    case RepositoryCacheMode.Default:
                        provider = GlobalCache.IsolatedRuntimeCache;
                        break;
                    case RepositoryCacheMode.Scoped:
                        provider = scope.IsolatedRuntimeCache;
                        break;
                    default:
                        throw new Exception("oops: cache mode.");
                }

                return _isolatedCache = GetIsolatedCache(provider);
            }
        }

        protected virtual IRuntimeCacheProvider GetIsolatedCache(IsolatedRuntimeCache provider)
        {
            return provider.GetOrCreateCache<TEntity>();
        }

        // this is a *bad* idea because PerformCount captures the current repository and its UOW
        //
        //private static RepositoryCachePolicyOptions _defaultOptions;
        //protected virtual RepositoryCachePolicyOptions DefaultOptions
        //{
        //    get
        //    {
        //        return _defaultOptions ?? (_defaultOptions
        //            = new RepositoryCachePolicyOptions(() =>
        //            {
        //                // get count of all entities of current type (TEntity) to ensure cached result is correct
        //                // create query once if it is needed (no need for locking here) - query is static!
        //                var query = _hasIdQuery ?? (_hasIdQuery = Query<TEntity>.Builder.Where(x => x.Id != 0));
        //                return PerformCount(query);
        //            }));
        //    }
        //}

        protected virtual RepositoryCachePolicyOptions DefaultOptions
        {
            get
            {
                return new RepositoryCachePolicyOptions(() =>
                    {
                        // get count of all entities of current type (TEntity) to ensure cached result is correct
                        // create query once if it is needed (no need for locking here) - query is static!
                        var query = _hasIdQuery ?? (_hasIdQuery = Query<TEntity>.Builder.Where(x => x.Id != 0));
                        return PerformCount(query);
                    });
            }
        }

        // this would be better for perfs BUT it breaks the tests - l8tr
        //
        //private static IRepositoryCachePolicy<TEntity, TId> _defaultCachePolicy;
        //protected virtual IRepositoryCachePolicy<TEntity, TId> DefaultCachePolicy
        //{
        //    get
        //    {
        //        return _defaultCachePolicy ?? (_defaultCachePolicy
        //            = new DefaultRepositoryCachePolicy<TEntity, TId>(IsolatedCache, DefaultOptions));
        //    }
        //}

        private IRepositoryCachePolicy<TEntity, TId> _cachePolicy;
        protected IRepositoryCachePolicy<TEntity, TId> CachePolicy
        {
            get
            {
                if (_cachePolicy != null) return _cachePolicy;

                if (GlobalCache == CacheHelper.NoCache)
                    return _cachePolicy = NoRepositoryCachePolicy<TEntity, TId>.Instance;
                
                // create the cache policy using IsolatedCache which is either global
                // or scoped depending on the repository cache mode for the current scope
                _cachePolicy = CreateCachePolicy(IsolatedCache);
                var scope = UnitOfWork.Scope;
                switch (scope.RepositoryCacheMode)
                {
                    case RepositoryCacheMode.Default:
                        break;
                    case RepositoryCacheMode.Scoped:
                        var globalIsolatedCache = GetIsolatedCache(GlobalCache.IsolatedRuntimeCache);
                        _cachePolicy = _cachePolicy.Scoped(globalIsolatedCache, scope);
                        break;
                    default:
                        throw new Exception("oops: cache mode.");
                }

                return _cachePolicy;
            }
        }

        protected virtual IRepositoryCachePolicy<TEntity, TId> CreateCachePolicy(IRuntimeCacheProvider runtimeCache)
        {
            return new DefaultRepositoryCachePolicy<TEntity, TId>(runtimeCache, DefaultOptions);
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

            //TODO: In v8 we should automatically reset dirty properties so they don't have to be manually reset in all of the implemented repositories
            //if (entity is ICanBeDirty dirty) dirty.ResetDirtyProperties();
        }

        /// <summary>
        /// Unit of work method that tells the repository to persist the updated entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PersistUpdatedItem(IEntity entity)
        {
            CachePolicy.Update((TEntity) entity, PersistUpdatedItem);
            //TODO: In v8 we should automatically reset dirty properties so they don't have to be manually reset in all of the implemented repositories
            //if (entity is ICanBeDirty dirty) dirty.ResetDirtyProperties();
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
