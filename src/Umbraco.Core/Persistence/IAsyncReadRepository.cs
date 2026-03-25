namespace Umbraco.Cms.Core.Persistence;

public interface IAsyncReadRepository<in TKey, TEntity> : IRepository
{
    /// <summary>
    ///     Gets an entity.
    /// </summary>
    Task<TEntity?> GetAsync(TKey? key, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetManyAsync(TKey[] keys, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a value indicating whether an entity exists.
    /// </summary>
    Task<bool> ExistsAsync(TKey key, CancellationToken cancellationToken);
}
