using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Cache
{
    internal class ScopedDefaultRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        private readonly DefaultRepositoryCachePolicy<TEntity, TId> _cachePolicy;
        private readonly IRuntimeCacheProvider _globalIsolatedCache;
        private readonly IScope _scope;

        public ScopedDefaultRepositoryCachePolicy(DefaultRepositoryCachePolicy<TEntity, TId> cachePolicy, IRuntimeCacheProvider globalIsolatedCache, IScope scope)
        {
            _cachePolicy = cachePolicy;
            _globalIsolatedCache = globalIsolatedCache;
            _scope = scope;
        }

        // when the scope completes we need to clear the global isolated cache
        // for now, we are not doing it selectively at all - just kill everything
        private void RegisterDirty(TEntity entity = null)
        {
            // "name" would be used to de-duplicate?
            // fixme - casting!
            ((Scope) _scope).Register("name", completed => _globalIsolatedCache.ClearAllCache());
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
            RegisterDirty(entity);
        }

        public void Update(TEntity entity, Action<TEntity> persistUpdated)
        {
            // writes into the local cache
            _cachePolicy.Update(entity, persistUpdated);
            RegisterDirty(entity);
        }

        public void Delete(TEntity entity, Action<TEntity> persistDeleted)
        {
            // deletes the local cache
            _cachePolicy.Delete(entity, persistDeleted);
            RegisterDirty(entity);
        }

        public TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            // loads into the local cache only, ok for now
            return _cachePolicy.GetAll(ids, performGetAll);
        }

        public void ClearAll()
        {
            // fixme - what's this doing?
            _cachePolicy.ClearAll();
            RegisterDirty();
        }
    }
}
