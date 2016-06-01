using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A base class for repository cache policies.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    internal abstract class RepositoryCachePolicyBase<TEntity, TId> : DisposableObject, IRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private Action _action;

        protected RepositoryCachePolicyBase(IRuntimeCacheProvider cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            Cache = cache;
        }

        protected IRuntimeCacheProvider Cache { get; }

        /// <summary>
        /// Disposing performs the actual caching action.
        /// </summary>
        protected override void DisposeResources()
        {
            _action?.Invoke();
        }

        /// <summary>
        /// Sets the action to execute when being disposed.
        /// </summary>
        /// <param name="action">An action to perform when being disposed.</param>
        protected void SetCacheAction(Action action)
        {
            _action = action;
        }

        /// <inheritdoc />
        public abstract TEntity Get(TId id, Func<TId, TEntity> repoGet);

        /// <inheritdoc />
        public abstract TEntity Get(TId id);

        /// <inheritdoc />
        public abstract bool Exists(TId id, Func<TId, bool> repoExists);

        /// <inheritdoc />
        public abstract void CreateOrUpdate(TEntity entity, Action<TEntity> repoCreateOrUpdate);

        /// <inheritdoc />
        public abstract void Remove(TEntity entity, Action<TEntity> repoRemove);

        /// <inheritdoc />
        public abstract TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> repoGet);
    }
}