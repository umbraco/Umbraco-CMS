namespace Umbraco.Cms.Core.Persistence
{
    /// <summary>
    /// Defines the base implementation of a reading repository.
    /// </summary>
    public interface IAsyncReadRepository<in TId, TEntity> : IReadRepository<TId, TEntity>
    {
        /// <summary>
        /// Gets an entity.
        /// </summary>
        Task<TEntity?> GetAsync(TId? id);

        /// <summary>
        /// Gets a value indicating whether an entity exists.
        /// </summary>
        Task<bool> ExistsAsync(TId id);
    }
}
