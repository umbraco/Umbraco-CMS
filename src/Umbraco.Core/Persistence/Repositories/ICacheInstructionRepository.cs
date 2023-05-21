using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="ICacheInstruction" /> entities.
/// </summary>
public interface ICacheInstructionRepository : IRepository
{
    /// <summary>
    ///     Gets the count of pending cache instruction records.
    /// </summary>
    int CountAll();

    /// <summary>
    ///     Gets the count of pending cache instructions.
    /// </summary>
    int CountPendingInstructions(int lastId);

    /// <summary>
    ///     Gets the most recent cache instruction record Id.
    /// </summary>
    /// <returns></returns>
    int GetMaxId();

    /// <summary>
    ///     Checks to see if a single cache instruction by Id exists.
    /// </summary>
    bool Exists(int id);

    /// <summary>
    ///     Adds a new cache instruction record.
    /// </summary>
    void Add(CacheInstruction cacheInstruction);

    /// <summary>
    ///     Gets a collection of cache instructions created later than the provided Id.
    /// </summary>
    /// <param name="lastId">Last id processed.</param>
    /// <param name="maxNumberToRetrieve">The maximum number of instructions to retrieve.</param>
    IEnumerable<CacheInstruction> GetPendingInstructions(int lastId, int maxNumberToRetrieve);

    /// <summary>
    ///     Deletes cache instructions older than the provided date.
    /// </summary>
    void DeleteInstructionsOlderThan(DateTime pruneDate);
}
