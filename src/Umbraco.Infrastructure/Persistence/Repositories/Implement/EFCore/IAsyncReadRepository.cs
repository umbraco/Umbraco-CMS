using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

public interface IAsyncReadRepository<in TId, TEntity> : IRepository
{
    /// <summary>
    ///     Gets an entity.
    /// </summary>
    Task<TEntity?> GetAsync(TId? id);

    /// <summary>
    ///     Gets entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetManyAsync(params TId[]? ids);

    /// <summary>
    ///     Gets a value indicating whether an entity exists.
    /// </summary>
    Task<bool> ExistsAsync(TId id);
}
