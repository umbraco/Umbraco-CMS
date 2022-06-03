namespace Umbraco.Cms.Core.Persistence
{
    /// <summary>
    /// Defines the base implementation of a writing repository.
    /// </summary>
    public interface IAsyncWriteRepository<in TEntity> : IWriteRepository<TEntity>
    {
        /// <summary>
        /// Saves an entity.
        /// </summary>
        Task SaveAsync(TEntity entity);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity"></param>
        Task DeleteAsync(TEntity entity)
        {
            Delete(entity);
            return Task.CompletedTask;
        }
    }
}
