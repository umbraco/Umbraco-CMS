// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

/// <summary>
///     Implements periodic database instruction processing as a hosted service.
/// </summary>
public class InstructionProcessJob : RecurringBackgroundJobBase
{
    /// <summary>
    /// Gets the delay time before the job is executed. The delay is fixed at one minute.
    /// </summary>
    public override TimeSpan Delay => TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets an array containing all possible values of the <see cref="ServerRole"/> enumeration.
    /// </summary>
    public override ServerRole[] ServerRoles => Enum.GetValues<ServerRole>();

    private readonly ILogger<InstructionProcessJob> _logger;
    private readonly IServerMessenger _messenger;
    private readonly TimeSpan _syncTimeout;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstructionProcessJob" /> class.
    /// </summary>
    /// <param name="messenger">Service broadcasting cache notifications to registered servers.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="globalSettings">The configuration for global settings.</param>
    public InstructionProcessJob(
        IServerMessenger messenger,
        ILogger<InstructionProcessJob> logger,
        IOptions<GlobalSettings> globalSettings)
        : base(globalSettings.Value.DatabaseServerMessenger.TimeBetweenSyncOperations)
    {
        _messenger = messenger;
        _logger = logger;
        _syncTimeout = ValidateSyncTimeout(globalSettings.Value.DatabaseServerMessenger.SyncTimeout);
    }

    // A non-positive timeout would make every sync "time out" immediately (or throw from WaitAsync for a
    // negative value), so guard against misconfiguration and fall back to the default. Timeout.InfiniteTimeSpan
    // is allowed as an explicit opt-out that restores the unbounded wait.
    private TimeSpan ValidateSyncTimeout(TimeSpan configuredSyncTimeout)
    {
        if (configuredSyncTimeout > TimeSpan.Zero || configuredSyncTimeout == Timeout.InfiniteTimeSpan)
        {
            return configuredSyncTimeout;
        }

        _logger.LogWarning(
            "Configured DatabaseServerMessenger.SyncTimeout of {ConfiguredSyncTimeout} is not valid; it must be positive (or Timeout.InfiniteTimeSpan to disable the timeout). Falling back to {DefaultSyncTimeout}.",
            configuredSyncTimeout,
            DatabaseServerMessengerSettings.DefaultSyncTimeout);

        return DatabaseServerMessengerSettings.DefaultSyncTimeout;
    }

    /// <summary>
    /// Executes the instruction processing job asynchronously by synchronizing messages using the messenger service.
    /// Logs an error if the synchronization fails or stalls, but always completes the task so polling continues.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that is signaled when the host is shutting down.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public override async Task RunJobAsync(CancellationToken cancellationToken)
    {
        // IServerMessenger.Sync() is synchronous and cannot observe the cancellation token, so a hung database
        // connection would otherwise block this job's recurring loop indefinitely and silently stop cache
        // polling until the process is recycled. Offload it to the thread pool and bound the wait so the loop
        // survives and keeps polling.
        //
        // This bounds the loop, not the stalled sync itself: the abandoned call still holds
        // DatabaseServerMessenger's sync lock, so polls stay no-ops until that call finally faults (bounded by
        // the database command/connection timeout, not by SyncTimeout), after which syncing resumes on its own
        // without a recycle.
        //
        // The loop is already started under ExecutionContext.SuppressFlow() (see RecurringBackgroundJobHostedService.StartAsync),
        // which is what makes offloading the scope-creating Sync() to Task.Run safe for the static ambient scope stack.
        var syncTask = Task.Run(_messenger.Sync, cancellationToken);

        try
        {
            await syncTask.WaitAsync(_syncTimeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            _logger.LogError(
                "Cache instruction sync did not complete within {SyncTimeout} and may be stalled on a hung database connection. Cache updates are paused on this server until the stalled connection recovers.",
                _syncTimeout);

            // The stalled sync keeps running until its database connection faults; observe its eventual failure
            // so it does not surface as an UnobservedTaskException.
            _ = syncTask.ContinueWith(
                static t => _ = t.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            _logger.LogError(e, "Failed (will repeat).");
        }
    }
}
