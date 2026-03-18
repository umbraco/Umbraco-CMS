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
    private readonly ILogger<UnattendedUpgradeBackgroundService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnattendedUpgradeBackgroundService"/> class.
    /// </summary>
    /// <param name="runtimeState">The Umbraco runtime state.</param>
    /// <param name="eventAggregator">The event aggregator used to publish upgrade notifications.</param>
    /// <param name="components">The component collection to initialize after migration completes.</param>
    /// <param name="hostApplicationLifetime">The host application lifetime for registering started/stopped callbacks.</param>
    /// <param name="logger">The logger.</param>
    public UnattendedUpgradeBackgroundService(
        IRuntimeState runtimeState,
        IEventAggregator eventAggregator,
        ComponentCollection components,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<UnattendedUpgradeBackgroundService> logger)
    {
        _runtimeState = runtimeState;
        _eventAggregator = eventAggregator;
        _components = components;
        _hostApplicationLifetime = hostApplicationLifetime;
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

        try
        {
            await RunMigrationsAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unattended upgrade failed.");
            _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, ex);
            return;
        }

        // Re-evaluate runtime level after migrations complete. This handles all result cases:
        // - CoreUpgradeComplete / PackageMigrationComplete: confirms the new Run level.
        // - NotRequired: another instance may have already run migrations; re-check to get Run level.
        // - HasErrors: BootFailedException is set, so DetermineRuntimeLevel() returns early (no-op).
        DetermineRuntimeLevel();

        // RunMigrationsAsync may have set BootFailed via a non-throwing error path (HasErrors result).
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
