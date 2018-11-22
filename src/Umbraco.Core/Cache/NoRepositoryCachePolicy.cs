using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Cache
{
    internal class NoRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId> 
        where TEntity : class, IAggregateRoot
    {
        private static readonly NoRepositoryCachePolicy<TEntity, TId> StaticInstance = new NoRepositoryCachePolicy<TEntity, TId>();

        private NoRepositoryCachePolicy()
        { }

        public static NoRepositoryCachePolicy<TEntity, TId> Instance { get { return StaticInstance; } }

        public IRepositoryCachePolicy<TEntity, TId> Scoped(IRuntimeCacheProvider runtimeCache, IScope scope)
        {
            throw new NotImplementedException();
        }

        public TEntity Get(TId id, Func<TId, TEntity> performGet, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            return performGet(id);
        }

        public TEntity GetCached(TId id)
        {
            return null;
        }

        public bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            return performExists(id);
        }

        public void Create(TEntity entity, Action<TEntity> persistNew)
        {
            persistNew(entity);
        }

        public void Update(TEntity entity, Action<TEntity> persistUpdated)
        {
            persistUpdated(entity);
        }

        public void Delete(TEntity entity, Action<TEntity> persistDeleted)
        {
            persistDeleted(entity);
        }

        public TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            return performGetAll(ids).ToArray();
        }

        public void ClearAll()
        { }
    }
}
