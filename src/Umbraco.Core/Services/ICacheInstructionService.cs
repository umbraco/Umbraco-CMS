using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Services;

public interface ICacheInstructionService
{
    /// <summary>
    ///     Checks to see if a cold boot is required, either because instructions exist and none have been synced or
    ///     because the last recorded synced instruction can't be found in the database.
    /// </summary>
    bool IsColdBootRequired(int lastId);

    /// <summary>
    ///     Checks to see if the number of pending instructions are over the configured limit.
    /// </summary>
    bool IsInstructionCountOverLimit(int lastId, int limit, out int count);

    /// <summary>
    ///     Gets the most recent cache instruction record Id.
    /// </summary>
    /// <returns></returns>
    int GetMaxInstructionId();

    /// <summary>
    ///     Creates a cache instruction record from a set of individual instructions and saves it.
    /// </summary>
    void DeliverInstructions(IEnumerable<RefreshInstruction> instructions, string localIdentity);

    /// <summary>
    ///     Creates one or more cache instruction records based on the configured batch size from a set of individual
    ///     instructions and saves them.
    /// </summary>
    void DeliverInstructionsInBatches(IEnumerable<RefreshInstruction> instructions, string localIdentity);

    /// <summary>
    ///     Processes and then prunes pending database cache instructions.
    /// </summary>
    /// <param name="released">Flag indicating if process is shutting now and operations should exit.</param>
    /// <param name="localIdentity">Local identity of the executing AppDomain.</param>
    /// <param name="lastPruned">Date of last prune operation.</param>
    /// <param name="lastId">Id of the latest processed instruction</param>
    ProcessInstructionsResult ProcessInstructions(
        CacheRefresherCollection cacheRefreshers,
        ServerRole serverRole,
        CancellationToken cancellationToken,
        string localIdentity,
        DateTime lastPruned,
        int lastId);
}
