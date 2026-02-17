using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

public interface IAsyncWriteRepository<in TEntity> : IRepository
{
    /// <summary>
    ///     Saves an entity.
    /// </summary>
    Task SaveAsync(TEntity entity);

    /// <summary>
    ///     Deletes an entity.
    /// </summary>
    /// <param name="entity"></param>
    Task DeleteAsync(TEntity entity);
}
