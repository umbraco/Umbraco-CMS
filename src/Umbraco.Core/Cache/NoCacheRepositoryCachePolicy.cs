using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache;

public class NoCacheRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    private NoCacheRepositoryCachePolicy()
    {
    }

    public static NoCacheRepositoryCachePolicy<TEntity, TId> Instance { get; } = new();

    public TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll) =>
        performGet(id);

    public TEntity? GetCached(TId id) => null;

    public bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>?> performGetAll) =>
        performExists(id);

    public void Create(TEntity entity, Action<TEntity> persistNew) => persistNew(entity);

    public void Update(TEntity entity, Action<TEntity> persistUpdated) => persistUpdated(entity);

    public void Delete(TEntity entity, Action<TEntity> persistDeleted) => persistDeleted(entity);

    public TEntity[] GetAll(TId[]? ids, Func<TId[]?, IEnumerable<TEntity>?> performGetAll) =>
        performGetAll(ids)?.ToArray() ?? Array.Empty<TEntity>();

    public void ClearAll()
    {
    }
}
