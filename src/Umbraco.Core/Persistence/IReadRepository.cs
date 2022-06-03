using System.Collections.Generic;

namespace Umbraco.Cms.Core.Persistence
{
    /// <summary>
    /// Defines the base implementation of a reading repository.
    /// </summary>
    public interface IReadRepository<in TId, TEntity> : IRepository
    {
        /// <summary>
        /// Gets an entity.
        /// </summary>
        TEntity? Get(TId? id);

        /// <summary>
        /// Gets an entity.
        /// </summary>
        Task<TEntity?> GetAsync(TId? id);

        /// <summary>
        /// Gets entities.
        /// </summary>
        IEnumerable<TEntity> GetMany(params TId[]? ids);

        /// <summary>
        /// Gets a value indicating whether an entity exists.
        /// </summary>
        bool Exists(TId id);

        /// <summary>
        /// Gets a value indicating whether an entity exists.
        /// </summary>
        Task<bool> ExistsAsync(TId id);
    }
}
