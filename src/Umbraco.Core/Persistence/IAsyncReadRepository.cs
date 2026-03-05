namespace Umbraco.Cms.Core.Persistence;

public interface IAsyncReadRepository<in TId, TEntity> : IRepository
{
    /// <summary>
    ///     Gets an entity.
    /// </summary>
    Task<TEntity?> GetAsync(TId? id, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetManyAsync(TId[] ids, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether an entity exists.
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken);
}
