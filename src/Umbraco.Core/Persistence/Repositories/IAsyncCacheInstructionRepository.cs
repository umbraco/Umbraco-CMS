using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for <see cref="ICacheInstruction"/> entities.
    /// </summary>
    public interface IAsyncCacheInstructionRepository : ICacheInstructionRepository
    {
        /// <summary>
        /// Gets the count of pending cache instruction records.
        /// </summary>
        Task<int> CountAllAsync();

        /// <summary>
        /// Gets the count of pending cache instructions.
        /// </summary>
        Task<int> CountPendingInstructionsAsync(int lastId);

        /// <summary>
        /// Gets the most recent cache instruction record Id.
        /// </summary>
        /// <returns></returns>
        Task<int> GetMaxIdAsync();

        /// <summary>
        /// Checks to see if a single cache instruction by Id exists.
        /// </summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Adds a new cache instruction record.
        /// </summary>
        Task AddAsync(CacheInstruction cacheInstruction);

        /// <summary>
        /// Gets a collection of cache instructions created later than the provided Id.
        /// </summary>
        /// <param name="lastId">Last id processed.</param>
        /// <param name="maxNumberToRetrieve">The maximum number of instructions to retrieve.</param>
        Task<IEnumerable<CacheInstruction>> GetPendingInstructionsAsync(int lastId, int maxNumberToRetrieve);

        /// <summary>
        /// Deletes cache instructions older than the provided date.
        /// </summary>
        Task DeleteInstructionsOlderThanAsync(DateTime pruneDate);
    }
}
