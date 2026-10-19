// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Sync;

/// <summary>
/// Shared, bounded, non-throwing attempt to touch (register) the current server. Used by both
/// <see cref="Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration.TouchServerJob"/>'s recurring cadence and <see cref="Runtime.CoreRuntime"/>'s one-time attempt to
/// resolve the server role before boot notifications fire.
/// </summary>
internal static class BoundedServerTouch
{
    /// <summary>
    /// Resolves a server address and attempts to touch (register) the current server, bounded by the configured
    /// touch timeout so a hung database connection cannot block the caller indefinitely.
    /// </summary>
    /// <param name="registrationService">The service used to touch (register) the server.</param>
    /// <param name="hostingEnvironment">Used to resolve the server's application URL, if known.</param>
    /// <param name="globalSettings">Supplies the stale-server and touch timeouts.</param>
    /// <param name="logger">The caller's logger, so log entries are attributed to the caller.</param>
    /// <param name="cancellationToken">A cancellation token observed while waiting for the touch to complete.</param>
    /// <returns>
    /// The underlying, unbounded database-write task - not merely the bounded wait above, which has already been
    /// awaited (and any failure logged) by the time this method returns. A caller that needs to detect whether a
    /// previous touch is still stuck (e.g. a recurring job avoiding overlapping attempts) can hold onto this and
    /// check <see cref="Task.IsCompleted"/> on a later call.
    /// </returns>
    /// <remarks>
    /// Never throws: a timeout or a failed write is caught and logged, leaving it to the caller's own retry
    /// cadence (or the lack thereof, for a one-time caller) to try again.
    /// </remarks>
    public static async Task<Task> TryTouchAsync(
        IServerRegistrationService registrationService,
        IHostingEnvironment hostingEnvironment,
        GlobalSettings globalSettings,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var serverAddress = ResolveServerAddress(hostingEnvironment, logger);
        TimeSpan touchTimeout = ValidateTouchTimeout(globalSettings.DatabaseServerRegistrar.TouchTimeout, logger);
        TimeSpan staleServerTimeout = globalSettings.DatabaseServerRegistrar.StaleServerTimeout;

        // IServerRegistrationService.TouchServer() runs a synchronous database write and cannot observe the
        // cancellation token, so a hung connection would otherwise block the caller indefinitely. Offload it to
        // the thread pool and bound the wait so the caller survives regardless of what the database does.
        //
        // Suppress execution context flow around the fork: this is shared infrastructure called from callers with
        // different ambient guarantees. TouchServerJob's loop already runs under suppression (see
        // RecurringBackgroundJobHostedService.StartAsync), but CoreRuntime and UnattendedUpgradeBackgroundService
        // don't provide that, and by the time either reaches here it has already run several notification handlers
        // that may hold a scope open. Without suppression, a fork that inherits a non-empty ambient scope stack
        // would share it with the caller's own subsequent scope pushes/pops - see
        // RecurringBackgroundJobHostedService.StartAsync for the original repro of that failure mode. The
        // IsFlowSuppressed() check avoids double-suppressing (and throwing) when already inside a suppressed
        // context, as TouchServerJob's is.
        Task touchTask;
        using (ExecutionContext.IsFlowSuppressed() ? null : (IDisposable?)ExecutionContext.SuppressFlow())
        {
            touchTask = Task.Run(() => registrationService.TouchServer(serverAddress, staleServerTimeout), cancellationToken);
        }

        // Observe the task's eventual fault on every exit path (timeout, shutdown cancellation, or a late
        // failure once the caller has stopped awaiting it) so it never surfaces as an UnobservedTaskException.
        _ = touchTask.ContinueWith(
            static t => _ = t.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        try
        {
            await touchTask.WaitAsync(touchTimeout, cancellationToken);
            logger.LogDebug("Touched server registration for {ServerAddress}.", serverAddress);
        }
        catch (TimeoutException)
        {
            logger.LogError(
                "Touching the server registration did not complete within {TouchTimeout} and may be stalled on a hung database connection. Server registration is paused on this server until the stalled connection recovers.",
                touchTimeout);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to update server record in database.");
        }

        return touchTask;
    }

    private static string ResolveServerAddress(IHostingEnvironment hostingEnvironment, ILogger logger)
    {
        var serverAddress = hostingEnvironment.ApplicationMainUrl?.ToString();
        if (string.IsNullOrWhiteSpace(serverAddress))
        {
            // No application URL is known yet: either detection is off (WebRouting:ApplicationUrlDetection is
            // None with no UmbracoApplicationUrl set), or detection is on but no request has been served yet.
            // Register with the machine name as a placeholder so server-role election can still proceed (uniqueness
            // comes from the server identity, not this address). If a URL is later detected from a request, the next
            // touch overwrites the placeholder.
            serverAddress = Environment.MachineName;
            logger.LogDebug(
                "No application URL available; registering server with placeholder address {ServerAddress}.",
                serverAddress);
        }
        else
        {
            logger.LogDebug("Registering server with application URL {ServerAddress}.", serverAddress);
        }

        return serverAddress;
    }

    // A non-positive timeout would make every touch "time out" immediately (or throw from WaitAsync for a
    // negative value), so guard against misconfiguration and fall back to the default. Timeout.InfiniteTimeSpan
    // is allowed as an explicit opt-out that restores the unbounded wait.
    private static TimeSpan ValidateTouchTimeout(TimeSpan configuredTouchTimeout, ILogger logger)
    {
        if (configuredTouchTimeout > TimeSpan.Zero || configuredTouchTimeout == Timeout.InfiniteTimeSpan)
        {
            return configuredTouchTimeout;
        }

        logger.LogWarning(
            "Configured DatabaseServerRegistrar.TouchTimeout of {ConfiguredTouchTimeout} is not valid; it must be positive (or Timeout.InfiniteTimeSpan to disable the timeout). Falling back to {DefaultTouchTimeout}.",
            configuredTouchTimeout,
            DatabaseServerRegistrarSettings.DefaultTouchTimeout);

        return DatabaseServerRegistrarSettings.DefaultTouchTimeout;
    }
}
