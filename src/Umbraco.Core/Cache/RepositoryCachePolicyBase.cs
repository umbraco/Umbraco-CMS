using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for repository cache policies.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    internal abstract class RepositoryCachePolicyBase<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        protected RepositoryCachePolicyBase(IRuntimeCacheProvider cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");            
            Cache = cache;
        }

        public abstract IRepositoryCachePolicy<TEntity, TId> Scoped(IRuntimeCacheProvider runtimeCache, IScope scope);

        protected IRuntimeCacheProvider Cache { get; private set; }

        /// <inheritdoc />
        public abstract TEntity Get(TId id, Func<TId, TEntity> performGet, Func<TId[], IEnumerable<TEntity>> performGetAll);

        /// <inheritdoc />
        public abstract TEntity GetCached(TId id);

        /// <inheritdoc />
        public abstract bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>> performGetAll);

        /// <inheritdoc />
        public abstract void Create(TEntity entity, Action<TEntity> persistNew);

        /// <inheritdoc />
        public abstract void Update(TEntity entity, Action<TEntity> persistUpdated);

        /// <inheritdoc />
        public abstract void Delete(TEntity entity, Action<TEntity> persistDeleted);

        /// <inheritdoc />
        public abstract TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> performGetAll);

        /// <inheritdoc />
        public abstract void ClearAll();

    }
}