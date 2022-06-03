using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache
{
    public class NoCacheRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
        where TEntity : class, IEntity
    {
        private NoCacheRepositoryCachePolicy() { }

        public static NoCacheRepositoryCachePolicy<TEntity, TId> Instance { get; } = new NoCacheRepositoryCachePolicy<TEntity, TId>();

        public TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
        {
            return performGet(id);
        }
        public async Task<TEntity?> GetAsync(TId? id, Func<TId?, Task<TEntity?>> performGetAsync, Func<TId[]?, Task<IEnumerable<TEntity>?>> performGetAllAsync)
        {
            return await performGetAsync(id);
        }

        public TEntity? GetCached(TId id)
        {
            return null;
        }

        public bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>?> performGetAll)
        {
            return performExists(id);
        }
        public async Task<bool> ExistsAsync(TId id, Func<TId, Task<bool>> performExistsAsync, Func<TId[], IEnumerable<TEntity>?> performGetAll)
        {
            return await performExistsAsync(id);
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

        public TEntity[] GetAll(TId[]? ids, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
        {
            return performGetAll(ids)?.ToArray() ?? Array.Empty<TEntity>();
        }

        public async Task<TEntity[]> GetAllAsync(TId[]? ids, Func<TId[]?, Task<IEnumerable<TEntity>?>> performGetAllAsync)
        {
            var result = await performGetAllAsync(ids);
            return result?.ToArray() ?? Array.Empty<TEntity>();
        }

        public void ClearAll()
        { }

    }
}
