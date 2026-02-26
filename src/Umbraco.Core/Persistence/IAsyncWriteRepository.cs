namespace Umbraco.Cms.Core.Persistence;

public interface IAsyncWriteRepository<in TEntity> : IRepository
{
    /// <summary>
    ///     Saves an entity.
    /// </summary>
    Task SaveAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes an entity.
    /// </summary>
    /// <param name="entity"></param>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
}
