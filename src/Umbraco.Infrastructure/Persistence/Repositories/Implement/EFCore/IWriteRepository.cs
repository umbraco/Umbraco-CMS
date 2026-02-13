using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

public interface IWriteRepository<in TEntity> : IRepository
{
    /// <summary>
    ///     Saves an entity.
    /// </summary>
    Task Save(TEntity entity);

    /// <summary>
    ///     Deletes an entity.
    /// </summary>
    /// <param name="entity"></param>
    Task Delete(TEntity entity);
}
