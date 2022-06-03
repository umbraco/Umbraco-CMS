using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence
{
    /// <summary>
    /// Defines the base implementation of a querying repository.
    /// </summary>
    public interface IAsyncQueryRepository<TEntity> : IQueryRepository<TEntity>
    {
        /// <summary>
        /// Gets entities.
        /// </summary>
        Task<IEnumerable<TEntity>> GetAsync(IQuery<TEntity> query)
        {
            return Task.FromResult(Get(query));
        }

        /// <summary>
        /// Counts entities.
        /// </summary>
        Task<int> CountAsync(IQuery<TEntity> query)
        {
            return Task.FromResult(Count(query));
        }
    }
}
