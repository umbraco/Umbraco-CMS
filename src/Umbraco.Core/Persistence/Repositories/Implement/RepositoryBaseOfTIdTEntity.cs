using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Provides a base class to all repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity managed by this repository.</typeparam>
    /// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
    internal abstract class RepositoryBase<TId, TEntity> : IReadWriteQueryRepository<TId, TEntity>, IUnitOfWorkRepository
        where TEntity : class, IAggregateRoot
    {
        private IRepositoryCachePolicy<TEntity, TId> _cachePolicy;
        private IRuntimeCacheProvider _isolatedCache;

        protected RepositoryBase(IScopeUnitOfWork work, CacheHelper cache, ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            UnitOfWork = work ?? throw new ArgumentNullException(nameof(work));
            GlobalCache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected IScopeUnitOfWork UnitOfWork { get; }

        protected CacheHelper GlobalCache { get; }

        protected ILogger Logger { get; }

        #region Static Queries

        private IQuery<TEntity> _hasIdQuery;

        #endregion

        protected virtual TId GetEntityId(TEntity entity)
        {
            return (TId) (object) entity.Id;
        }

        /// <summary>
        /// The runtime cache used for this repo by default is the isolated cache for this type
        /// </summary>
        protected IRuntimeCacheProvider IsolatedCache
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
                    case RepositoryCacheMode.None:
                        return new NullCacheProvider(); // fixme cache instance
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
                    var query = _hasIdQuery ?? (_hasIdQuery = UnitOfWork.SqlContext.Query<TEntity>().Where(x => x.Id != 0));
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

        protected virtual IRepositoryCachePolicy<TEntity, TId> CachePolicy
        {
            get
            {
                if (GlobalCache == CacheHelper.NoCache)
                    return _cachePolicy = NoCacheRepositoryCachePolicy<TEntity, TId>.Instance;

                // create the cache policy using IsolatedCache which is either global
                // or scoped depending on the repository cache mode for the current scope
                var scope = UnitOfWork.Scope;
                switch (scope.RepositoryCacheMode)
                {
                    case RepositoryCacheMode.Default:
                        _cachePolicy = CreateCachePolicy(IsolatedCache);
                        break;
                    case RepositoryCacheMode.Scoped:
                        _cachePolicy = CreateCachePolicy(IsolatedCache);
                        var globalIsolatedCache = GetIsolatedCache(GlobalCache.IsolatedRuntimeCache);
                        _cachePolicy = _cachePolicy.Scoped(globalIsolatedCache, scope);
                        break;
                    case RepositoryCacheMode.None:
                        _cachePolicy = NoCacheRepositoryCachePolicy<TEntity, TId>.Instance;
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
        public void Save(TEntity entity)
        {
            if (entity.HasIdentity == false)
                PersistNewItem(entity);
            else
                PersistUpdatedItem(entity);
        }

        /// <summary>
        /// Deletes the passed in entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            PersistDeletedItem(entity);
        }

        protected abstract TEntity PerformGet(TId id);
        protected abstract IEnumerable<TEntity> PerformGetAll(params TId[] ids);
        protected abstract IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query);
        protected abstract bool PerformExists(TId id);
        protected abstract int PerformCount(IQuery<TEntity> query);
        protected abstract void PersistNewItem(TEntity item);
        protected abstract void PersistUpdatedItem(TEntity item);
        protected abstract void PersistDeletedItem(TEntity item);


        /// <summary>
        /// Gets an entity by the passed in Id utilizing the repository's cache policy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity Get(TId id)
        {
            return CachePolicy.Get(id, PerformGet, PerformGetAll);
        }

        /// <summary>
        /// Gets all entities of type TEntity or a list according to the passed in Ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> GetMany(params TId[] ids)
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

        /// <summary>
        /// Gets a list of entities by the passed in query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<TEntity> Get(IQuery<TEntity> query)
        {
            return PerformGetByQuery(query)
                //ensure we don't include any null refs in the returned collection!
                .WhereNotNull();
        }

        /// <summary>
        /// Returns a boolean indicating whether an entity with the passed Id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(TId id)
        {
            return CachePolicy.Exists(id, PerformExists, PerformGetAll);
        }

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
    }
}
