using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing cache instructions in distributed cache scenarios.
/// </summary>
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
    ///     Processes pending database cache instructions.
    /// </summary>
    /// <param name="cacheRefreshers">Cache refreshers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="localIdentity">Local identity of the executing AppDomain.</param>
    /// <param name="lastId">Id of the latest processed instruction.</param>
    /// <returns>The processing result.</returns>
    ProcessInstructionsResult ProcessInstructions(
        CacheRefresherCollection cacheRefreshers,
        CancellationToken cancellationToken,
        string localIdentity,
        int lastId);

    /// <summary>
    /// Processes all pending database cache instructions using the provided cache refreshers.
    /// </summary>
    /// <param name="cacheRefreshers">The collection of cache refreshers to use for processing instructions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="localIdentity">The local identity of the executing AppDomain.</param>
    /// <returns>The result of processing all instructions.</returns>
    ProcessInstructionsResult ProcessAllInstructions(
        CacheRefresherCollection cacheRefreshers,
        CancellationToken cancellationToken,
        string localIdentity)
        => ProcessInstructions(
            cacheRefreshers,
            cancellationToken,
            localIdentity,
            StaticServiceProvider.Instance.GetRequiredService<ILastSyncedManager>().GetLastSyncedExternalAsync().GetAwaiter().GetResult() ?? 0);


    /// <summary>
    ///     Processes pending cache instructions from the database for the internal (repository) caches.
    /// </summary>
    /// <param name="cacheRefreshers">The collection of cache refreshers to use for processing instructions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="localIdentity">The local identity of the executing AppDomain.</param>
    /// <returns>The result of processing the internal instructions.</returns>
    ProcessInstructionsResult ProcessInternalInstructions(
        CacheRefresherCollection cacheRefreshers,
        CancellationToken cancellationToken,
        string localIdentity)
        => ProcessInstructions(
            cacheRefreshers,
            cancellationToken,
            localIdentity,
            StaticServiceProvider.Instance.GetRequiredService<ILastSyncedManager>().GetLastSyncedExternalAsync().GetAwaiter().GetResult() ?? 0);
}
