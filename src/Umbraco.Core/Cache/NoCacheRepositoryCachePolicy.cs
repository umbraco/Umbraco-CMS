using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    class NoCacheRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IAggregateRoot
    {
        public void ClearAll()
        {
            // nothing to clear - not caching
        }

        public void Create(TEntity entity, Action<TEntity> persistNew)
        {
            persistNew(entity);
        }

        public void Delete(TEntity entity, Action<TEntity> persistDeleted)
        {
            persistDeleted(entity);
        }

        public bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            return performExists(id);
        }

        public TEntity Get(TId id, Func<TId, TEntity> performGet, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            return performGet(id);
        }

        public TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> performGetAll)
        {
            return performGetAll(ids).ToArray();
        }

        public TEntity GetCached(TId id)
        {
            return null;
        }

        public void Update(TEntity entity, Action<TEntity> persistUpdated)
        {
            persistUpdated(entity);
        }
    }
}
