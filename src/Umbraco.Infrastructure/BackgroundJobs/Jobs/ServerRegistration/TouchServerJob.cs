// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.ServerRegistration;

/// <summary>
///     Implements periodic server "touching" (to mark as active/deactive) as a hosted service.
/// </summary>
public class TouchServerJob : RecurringBackgroundJobBase
{
    /// <summary>
    /// Gets the fixed delay interval of 15 seconds between executions of the touch server job.
    /// This interval determines how often the server registration is updated.
    /// </summary>
    public override TimeSpan Delay => TimeSpan.FromSeconds(15);

    /// <summary>
    /// Gets all server roles on which this job runs. This property returns every possible <see cref="ServerRole"/> value, indicating the job runs on all server roles.
    /// </summary>
    /// <remarks>Runs on all servers</remarks>
    public override ServerRole[] ServerRoles => Enum.GetValues<ServerRole>();

    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<TouchServerJob> _logger;
    private readonly IServerRegistrationService _serverRegistrationService;
    private readonly IServerRoleAccessor _serverRoleAccessor;
    private readonly IDisposable? _onChangeRegistration;
    private GlobalSettings _globalSettings;
    private TimeSpan _touchTimeout;
    private Task? _inFlightTouch;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TouchServerJob" /> class.
    /// </summary>
    /// <param name="serverRegistrationService">Services for server registrations.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="globalSettings">The configuration for global settings.</param>
    /// <param name="hostingEnvironment">The hostingEnviroment.</param>
    /// <param name="serverRoleAccessor">The accessor for the server role</param>
    public TouchServerJob(
        IServerRegistrationService serverRegistrationService,
        IHostingEnvironment hostingEnvironment,
        ILogger<TouchServerJob> logger,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(globalSettings.CurrentValue.DatabaseServerRegistrar.WaitTimeBetweenCalls)
    {
        _serverRegistrationService = serverRegistrationService ?? throw new ArgumentNullException(nameof(serverRegistrationService));
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
        _globalSettings = globalSettings.CurrentValue;
        _serverRoleAccessor = serverRoleAccessor;
        _touchTimeout = ValidateTouchTimeout(globalSettings.CurrentValue.DatabaseServerRegistrar.TouchTimeout);

        _onChangeRegistration = globalSettings.OnChange(x =>
        {
            _globalSettings = x;
            Period = x.DatabaseServerRegistrar.WaitTimeBetweenCalls;
            _touchTimeout = ValidateTouchTimeout(x.DatabaseServerRegistrar.TouchTimeout);
        });
    }

    /// <summary>
    /// Executes the job that updates the server registration by touching the server record in the database.
    /// This keeps the server's registration active and ensures its status remains current.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that is signaled when the host is shutting down.</param>
    /// <returns>
    /// A completed task when the job has finished running.
    /// </returns>
    public override async Task RunJobAsync(CancellationToken cancellationToken)
    {
        // If the IServerRoleAccessor has been changed away from ElectedServerRoleAccessor this task no longer makes sense,
        // since all it's used for is to allow the ElectedServerRoleAccessor
        // to figure out what role a given server has, so we just stop this task.
        if (_serverRoleAccessor is not ElectedServerRoleAccessor)
        {
            return;
        }

        // If a previous touch is still running (e.g. blocked on a hung database connection after a timeout),
        // skip starting another. This bounds us to a single in-flight call instead of accumulating blocked
        // thread-pool threads (each contending for the servers lock), and logs the stall once rather than on
        // every interval until it recovers.
        if (_inFlightTouch is { IsCompleted: false })
        {
            return;
        }

        var serverAddress = _hostingEnvironment.ApplicationMainUrl?.ToString();
        if (string.IsNullOrWhiteSpace(serverAddress))
        {
            // No application URL is known yet: either detection is off (WebRouting:ApplicationUrlDetection is
            // None with no UmbracoApplicationUrl set), or detection is on but no request has been served yet.
            // Register with the machine name as a placeholder so server-role election can still proceed (uniqueness
            // comes from the server identity, not this address). If a URL is later detected from a request, the next
            // touch overwrites the placeholder.
            serverAddress = Environment.MachineName;
            _logger.LogDebug(
                "No application URL available; registering server with placeholder address {ServerAddress}.",
                serverAddress);
        }
        else
        {
            _logger.LogDebug("Registering server with application URL {ServerAddress}.", serverAddress);
        }

        // IServerRegistrationService.TouchServer() runs a synchronous database write and cannot observe the
        // cancellation token, so a hung connection would otherwise block this job's recurring loop indefinitely
        // and silently stop server-registration heartbeats until the process is recycled. Offload it to the
        // thread pool and bound the wait so the loop survives and keeps touching.
        // (See InstructionProcessJob for the same pattern and the ExecutionContext.SuppressFlow rationale.)
        TimeSpan staleServerTimeout = _globalSettings.DatabaseServerRegistrar.StaleServerTimeout;
        var touchTask = Task.Run(() => _serverRegistrationService.TouchServer(serverAddress, staleServerTimeout), cancellationToken);
        _inFlightTouch = touchTask;

        // Observe the task's eventual fault on every exit path (timeout, shutdown cancellation, or a late
        // failure once we have stopped awaiting it) so it never surfaces as an UnobservedTaskException.
        _ = touchTask.ContinueWith(
            static t => _ = t.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        try
        {
            await touchTask.WaitAsync(_touchTimeout, cancellationToken);
            _logger.LogDebug("Touched server registration for {ServerAddress}.", serverAddress);
        }
        catch (TimeoutException)
        {
            _logger.LogError(
                "Touching the server registration did not complete within {TouchTimeout} and may be stalled on a hung database connection. Server registration is paused on this server until the stalled connection recovers.",
                _touchTimeout);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update server record in database.");
        }
    }

    // A non-positive timeout would make every touch "time out" immediately (or throw from WaitAsync for a
    // negative value), so guard against misconfiguration and fall back to the default. Timeout.InfiniteTimeSpan
    // is allowed as an explicit opt-out that restores the unbounded wait.
    private TimeSpan ValidateTouchTimeout(TimeSpan configuredTouchTimeout)
    {
        if (configuredTouchTimeout > TimeSpan.Zero || configuredTouchTimeout == Timeout.InfiniteTimeSpan)
        {
            return configuredTouchTimeout;
        }

        _logger.LogWarning(
            "Configured DatabaseServerRegistrar.TouchTimeout of {ConfiguredTouchTimeout} is not valid; it must be positive (or Timeout.InfiniteTimeSpan to disable the timeout). Falling back to {DefaultTouchTimeout}.",
            configuredTouchTimeout,
            DatabaseServerRegistrarSettings.DefaultTouchTimeout);

        return DatabaseServerRegistrarSettings.DefaultTouchTimeout;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _onChangeRegistration?.Dispose();
        }

        base.Dispose(disposing);
    }
}
