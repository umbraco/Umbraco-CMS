using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using ComponentCollection = Umbraco.Cms.Core.Composing.ComponentCollection;

namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
/// Runs unattended database migrations as a background task so the HTTP server can start
/// immediately and respond to health probes during the upgrade.
/// </summary>
/// <remarks>
/// This service is only active when <see cref="RuntimeLevel.Upgrading"/> is set, which
/// happens when <c>UpgradeUnattended</c> or <c>PackageMigrationsUnattended</c> is true
/// and database migrations are pending.
/// </remarks>
internal sealed class UnattendedUpgradeBackgroundService : BackgroundService
{
    private readonly IRuntimeState _runtimeState;
    private readonly IEventAggregator _eventAggregator;
    private readonly ComponentCollection _components;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IMigrationCoordinator _coordinator;
    private readonly IRuntimeStartupReadinessControl _readiness;
    private readonly ILogger<UnattendedUpgradeBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnattendedUpgradeBackgroundService"/> class.
    /// </summary>
    /// <param name="runtimeState">The Umbraco runtime state.</param>
    /// <param name="eventAggregator">The event aggregator used to publish upgrade notifications.</param>
    /// <param name="components">The component collection to initialize after migration completes.</param>
    /// <param name="hostApplicationLifetime">The host application lifetime for registering started/stopped callbacks.</param>
    /// <param name="coordinator">Coordinates migration leadership across servers in a load-balanced environment.</param>
    /// <param name="readiness">Gates front-end serving until post-migration initialization has completed.</param>
    /// <param name="logger">The logger.</param>
    public UnattendedUpgradeBackgroundService(
        IRuntimeState runtimeState,
        IEventAggregator eventAggregator,
        ComponentCollection components,
        IHostApplicationLifetime hostApplicationLifetime,
        IMigrationCoordinator coordinator,
        IRuntimeStartupReadinessControl readiness,
        ILogger<UnattendedUpgradeBackgroundService> logger)
    {
        _runtimeState = runtimeState;
        _eventAggregator = eventAggregator;
        _components = components;
        _hostApplicationLifetime = hostApplicationLifetime;
        _coordinator = coordinator;
        _readiness = readiness;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Only run when an unattended upgrade is pending.
        if (_runtimeState.Level != RuntimeLevel.Upgrading)
        {
            return;
        }

        _logger.LogInformation("Unattended upgrade background service started.");

        // Keep the front end gated for the entire background finalization, re-opening only once
        // post-migration initialization (components + application-starting handlers, e.g. document URL
        // routing) has completed. This must be set BEFORE TryBecomeLeaderAsync: on a follower the level
        // flips to Run while polling inside that call, so the gate has to already be closed by then.
        _readiness.SetNotReady();

        bool isLeader = false;

        try
        {
            try
            {
                isLeader = await _coordinator.TryBecomeLeaderAsync(stoppingToken);

                if (_runtimeState.Level == RuntimeLevel.BootFailed)
                {
                    return;
                }

                if (isLeader)
                {
                    // Belt-and-suspenders for graceful shutdowns (e.g. Azure SIGTERM): release the claim
                    // as soon as the host begins stopping, even if a migration step is still blocking.
                    _hostApplicationLifetime.ApplicationStopping.Register(() => _coordinator.ReleaseLeadership());
                    await RunMigrationsAsync(stoppingToken);
                }
                else
                {
                    // Follower: rebuild per-server in-memory navigation and publish status
                    // from the fully-migrated database.
                    await _eventAggregator.PublishAsync(new PostRuntimePremigrationsUpgradeNotification(), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unattended upgrade failed.");
                _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, ex);
                return;
            }
            finally
            {
                // Always release the claim — even on leader failure — so other servers
                // can detect completion or take over.
                if (isLeader)
                {
                    _coordinator.ReleaseLeadership();
                }
            }

            // For the leader: confirms migrations succeeded and level transitions to Run.
            // For followers: level is already Run (set during TryBecomeLeaderAsync polling).
            DetermineRuntimeLevel();

            if (_runtimeState.Level == RuntimeLevel.BootFailed)
            {
                return;
            }

            // Migrations complete: initialize components and fire application lifecycle events.
            try
            {
                await _components.InitializeAsync(false, stoppingToken);
                await _eventAggregator.PublishAsync(new UmbracoApplicationStartingNotification(_runtimeState.Level, false), stoppingToken);

                _hostApplicationLifetime.ApplicationStarted.Register(
                    () => _eventAggregator.Publish(new UmbracoApplicationStartedNotification(false)));
                _hostApplicationLifetime.ApplicationStopped.Register(
                    () => _eventAggregator.Publish(new UmbracoApplicationStoppedNotification(false)));

                _logger.LogInformation("Unattended upgrade completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unattended upgrade post-migration initialization failed.");
                _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, ex);
            }
        }
        finally
        {
            // Always re-open the gate (on every exit path) so it can never wedge closed. On BootFailed the
            // maintenance filter no longer matters because BootFailedMiddleware intercepts the request first.
            _readiness.SetReady();
        }
    }

    private async Task RunMigrationsAsync(CancellationToken stoppingToken)
    {
        // Step 1: Premigrations upgrade.
        var premigrationNotification = new RuntimePremigrationsUpgradeNotification();
        await _eventAggregator.PublishAsync(premigrationNotification, stoppingToken);

        switch (premigrationNotification.UpgradeResult)
        {
            case RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.HasErrors:
                if (_runtimeState.BootFailedException is null)
                {
                    throw new InvalidOperationException(
                        $"Premigration upgrade result was {RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.HasErrors} but no {nameof(BootFailedException)} was registered.");
                }

                _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException);
                _logger.LogError(_runtimeState.BootFailedException, "Premigration upgrade failed.");
                return;

            case RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.CoreUpgradeComplete:
            case RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.NotRequired:
                break;
        }

        if (_runtimeState.Level == RuntimeLevel.BootFailed)
        {
            return;
        }

        // Step 2: Post-premigrations (navigation and publish-status initialization).
        await _eventAggregator.PublishAsync(new PostRuntimePremigrationsUpgradeNotification(), stoppingToken);

        // Step 3: Unattended upgrade (main migrations and package migrations).
        var upgradeNotification = new RuntimeUnattendedUpgradeNotification();
        await _eventAggregator.PublishAsync(upgradeNotification, stoppingToken);

        switch (upgradeNotification.UnattendedUpgradeResult)
        {
            case RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors:
                if (_runtimeState.BootFailedException is null)
                {
                    throw new InvalidOperationException(
                        $"Unattended upgrade result was {RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors} but no {nameof(BootFailedException)} was registered.");
                }

                _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException);
                _logger.LogError(_runtimeState.BootFailedException, "Unattended upgrade failed.");
                return;

            case RuntimeUnattendedUpgradeNotification.UpgradeResult.CoreUpgradeComplete:
            case RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete:
            case RuntimeUnattendedUpgradeNotification.UpgradeResult.NotRequired:
                break;
        }
    }

    private void DetermineRuntimeLevel()
    {
        // If a boot failure was already registered (e.g. by a premigration handler), there is
        // nothing more to determine — preserve the existing BootFailed state and return early,
        // matching the equivalent guard in CoreRuntime.DetermineRuntimeLevel().
        if (_runtimeState.BootFailedException is not null)
        {
            return;
        }

        try
        {
            _runtimeState.DetermineRuntimeLevel();
        }
        catch (Exception ex)
        {
            _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, ex);
            _logger.LogError(ex, "Failed to determine runtime level during unattended upgrade.");
        }
    }
}
