namespace Umbraco.Cms.Core.Persistence;

/// <summary>
///     Defines the base implementation of a reading repository.
/// </summary>
public interface IReadRepository<in TId, out TEntity> : IRepository
{
    /// <summary>
    ///     Gets an entity.
    /// </summary>
    TEntity? Get(TId? id);

    /// <summary>
    ///     Gets entities.
    /// </summary>
    IEnumerable<TEntity> GetMany(params TId[]? ids);

    /// <summary>
    ///     Gets a value indicating whether an entity exists.
    /// </summary>
    bool Exists(TId id);
}
