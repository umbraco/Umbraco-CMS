using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

public interface IReadRepository<in TId, TEntity> : IRepository
{
    /// <summary>
    ///     Gets an entity.
    /// </summary>
    Task<TEntity?> Get(TId? id);

    /// <summary>
    ///     Gets entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetMany(params TId[]? ids);

    /// <summary>
    ///     Gets a value indicating whether an entity exists.
    /// </summary>
    Task<bool> Exists(TId id);
}
