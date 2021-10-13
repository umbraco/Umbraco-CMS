using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Provides a base class to all repositories.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity managed by this repository.</typeparam>
    /// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
    internal abstract class RepositoryBase<TId, TEntity> : IReadWriteQueryRepository<TId, TEntity>
        where TEntity : class, IEntity
    {
        private IRepositoryCachePolicy<TEntity, TId> _cachePolicy;

        protected RepositoryBase(IScopeAccessor scopeAccessor, AppCaches appCaches, ILogger logger)
        {
            ScopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            AppCaches = appCaches ?? throw new ArgumentNullException(nameof(appCaches));
        }

        protected ILogger Logger { get; }

        protected AppCaches AppCaches { get; }

        protected IAppPolicyCache GlobalIsolatedCache => AppCaches.IsolatedCaches.GetOrCreate<TEntity>();

        protected IScopeAccessor ScopeAccessor { get; }

        protected IScope AmbientScope
        {
            get
            {
                var scope = ScopeAccessor.AmbientScope;
                if (scope == null)
                    throw new InvalidOperationException("Cannot run a repository without an ambient scope.");
                return scope;
            }
        }

        #region Static Queries

        private IQuery<TEntity> _hasIdQuery;

        #endregion

        protected virtual TId GetEntityId(TEntity entity)
        {
            return (TId) (object) entity.Id;
        }

        /// <summary>
        /// Gets the isolated cache.
        /// </summary>
        /// <remarks>Depends on the ambient scope cache mode.</remarks>
        protected IAppPolicyCache IsolatedCache
        {
            get
            {
                switch (AmbientScope.RepositoryCacheMode)
                {
                    case RepositoryCacheMode.Default:
                        return AppCaches.IsolatedCaches.GetOrCreate<TEntity>();
                    case RepositoryCacheMode.Scoped:
                        return AmbientScope.IsolatedCaches.GetOrCreate<TEntity>();
                    case RepositoryCacheMode.None:
                        return NoAppCache.Instance;
                    default:
                        throw new Exception("oops: cache mode.");
                }
            }
        }

        // ReSharper disable once StaticMemberInGenericType
        private static RepositoryCachePolicyOptions _defaultOptions;
        // ReSharper disable once InconsistentNaming
        protected virtual RepositoryCachePolicyOptions DefaultOptions
        {
            get
            {
                return _defaultOptions ?? (_defaultOptions
                    = new RepositoryCachePolicyOptions(() =>
                    {
                        // get count of all entities of current type (TEntity) to ensure cached result is correct
                        // create query once if it is needed (no need for locking here) - query is static!
                        var query = _hasIdQuery ?? (_hasIdQuery = AmbientScope.SqlContext.Query<TEntity>().Where(x => x.Id != 0));
                        return PerformCount(query);
                    }));
            }
        }

        protected IRepositoryCachePolicy<TEntity, TId> CachePolicy
        {
            get
            {
                if (AppCaches == AppCaches.NoCache)
                    return NoCacheRepositoryCachePolicy<TEntity, TId>.Instance;

                // create the cache policy using IsolatedCache which is either global
                // or scoped depending on the repository cache mode for the current scope

                switch (AmbientScope.RepositoryCacheMode)
                {
                    case RepositoryCacheMode.Default:
                    case RepositoryCacheMode.Scoped:
                        // return the same cache policy in both cases - the cache policy is
                        // supposed to pick either the global or scope cache depending on the
                        // scope cache mode
                        return _cachePolicy ?? (_cachePolicy = CreateCachePolicy());
                    case RepositoryCacheMode.None:
                        return NoCacheRepositoryCachePolicy<TEntity, TId>.Instance;
                    default:
                        throw new Exception("oops: cache mode.");
                }
            }
        }

        protected virtual IRepositoryCachePolicy<TEntity, TId> CreateCachePolicy()
        {
            return new DefaultRepositoryCachePolicy<TEntity, TId>(GlobalIsolatedCache, ScopeAccessor, DefaultOptions);
        }

        /// <summary>
        /// Adds or Updates an entity of type TEntity
        /// </summary>
        /// <remarks>This method is backed by an <see cref="IAppPolicyCache"/> cache</remarks>
        /// <param name="entity"></param>
        public void Save(TEntity entity)
        {
            if (entity.HasIdentity == false)
                CachePolicy.Create(entity, PersistNewItem);
            else
                CachePolicy.Update(entity, PersistUpdatedItem);
        }

        /// <summary>
        /// Deletes the passed in entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            CachePolicy.Delete(entity, PersistDeletedItem);
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
                // TODO: I think we should enabled this in case accidental calls are made to get all with invalid ids
                //.Where(x => Equals(x, default(TId)) == false)
                .ToArray();

            // can't query more than 2000 ids at a time... but if someone is really querying 2000+ entities,
            // the additional overhead of fetching them in groups is minimal compared to the lookup time of each group
            if (ids.Length <= Constants.Sql.MaxParameterCount)
            {
                return CachePolicy.GetAll(ids, PerformGetAll);
            }

            var entities = new List<TEntity>();
            foreach (var group in ids.InGroupsOf(Constants.Sql.MaxParameterCount))
            {
                entities.AddRange(CachePolicy.GetAll(group.ToArray(), PerformGetAll));
            }

            return entities;
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
    }
}
