using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    internal abstract class RepositoryCachePolicyBase<TEntity, TId> : DisposableObject, IRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private Action _action;

        protected RepositoryCachePolicyBase(IRuntimeCacheProvider cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            
            Cache = cache;
        }

        protected IRuntimeCacheProvider Cache { get; private set; }

        /// <summary>
        /// The disposal performs the caching
        /// </summary>
        protected override void DisposeResources()
        {
            if (_action != null)
            {
                _action();
            }
        }

        /// <summary>
        /// Sets the action to execute on disposal
        /// </summary>
        /// <param name="action"></param>
        protected void SetCacheAction(Action action)
        {
            _action = action;
        }

        public abstract TEntity Get(TId id, Func<TId, TEntity> getFromRepo);
        public abstract TEntity Get(TId id);
        public abstract bool Exists(TId id, Func<TId, bool> getFromRepo);
        public abstract void CreateOrUpdate(TEntity entity, Action<TEntity> persistMethod);
        public abstract void Remove(TEntity entity, Action<TEntity> persistMethod);
        public abstract TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> getFromRepo);
    }
}