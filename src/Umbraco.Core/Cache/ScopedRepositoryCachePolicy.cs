using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Cache
{
    internal class ScopedRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private readonly IRepositoryCachePolicy<TEntity, TId> _cachePolicy;
        private readonly IRuntimeCacheProvider _globalIsolatedCache;
        private readonly IScope _scope;

        public ScopedRepositoryCachePolicy(IRepositoryCachePolicy<TEntity, TId> cachePolicy, IRuntimeCacheProvider globalIsolatedCache, IScope scope)
        {
            _cachePolicy = cachePolicy;
            _globalIsolatedCache = globalIsolatedCache;
            _scope = scope;
        }

        public IRepositoryCachePolicy<TEntity, TId> Scoped(IRuntimeCacheProvider runtimeCache, IScope scope)
        {
            throw new InvalidOperationException(); // obviously
        }

        // when the scope completes we need to clear the global isolated cache
        // for now, we are not doing it selectively at all - just kill everything
        // later on we might want to be more clever
        private void RegisterDirty()
        {
            // use unique names to de-duplicate
            // enlisting multiple times is not a problem
            _scope.Enlist("dirty_" + typeof (TEntity).Name, ActionTime.BeforeDispose,
                (actionTime, completed) => { if (completed) _globalIsolatedCache.ClearAllCache(); });
        }

        public TEntity Get(TId id, Func<TId, TEntity> performGet, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            // loads into the local cache only, ok for now
            return _cachePolicy.Get(id, performGet, performGetAll);
        }

        public TEntity GetCached(TId id)
        {
            // loads into the local cache only, ok for now
            return _cachePolicy.GetCached(id);
        }

        public bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            // loads into the local cache only, ok for now
            return _cachePolicy.Exists(id, performExists, performGetAll);
        }

        public void Create(TEntity entity, Action<TEntity> persistNew)
        {
            // writes into the local cache
            _cachePolicy.Create(entity, persistNew);
            RegisterDirty();
        }

        public void Update(TEntity entity, Action<TEntity> persistUpdated)
        {
            // writes into the local cache
            _cachePolicy.Update(entity, persistUpdated);
            RegisterDirty();
        }

        public void Delete(TEntity entity, Action<TEntity> persistDeleted)
        {
            // deletes the local cache
            _cachePolicy.Delete(entity, persistDeleted);
            RegisterDirty();
        }

        public TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            // loads into the local cache only, ok for now
            return _cachePolicy.GetAll(ids, performGetAll);
        }

        public void ClearAll()
        {
            // clears the local cache
            _cachePolicy.ClearAll();
            RegisterDirty();
        }
    }
}
