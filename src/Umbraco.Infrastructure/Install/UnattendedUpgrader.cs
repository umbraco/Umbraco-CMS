using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
///     Handles <see cref="RuntimeUnattendedUpgradeNotification" /> to execute the unattended Umbraco upgrader
///     or the unattended Package migrations runner.
/// </summary>
public class UnattendedUpgrader : INotificationAsyncHandler<RuntimeUnattendedUpgradeNotification>
{
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly PackageMigrationRunner _packageMigrationRunner;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IRuntimeState _runtimeState;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly UnattendedSettings _unattendedSettings;
    private readonly DistributedCache _distributedCache;
    private readonly ILogger<UnattendedUpgrader> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Install.UnattendedUpgrader"/> class, responsible for performing unattended upgrades of the Umbraco CMS database and executing package migrations.
    /// </summary>
    /// <param name="profilingLogger">The logger used for profiling and logging upgrade operations.</param>
    /// <param name="umbracoVersion">Provides information about the current Umbraco version.</param>
    /// <param name="databaseBuilder">Handles database schema creation and upgrades.</param>
    /// <param name="runtimeState">Represents the current runtime state of the Umbraco application.</param>
    /// <param name="packageMigrationRunner">Executes package migrations during the upgrade process.</param>
    /// <param name="unattendedSettings">The configuration options for unattended upgrades.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public UnattendedUpgrader(
        IProfilingLogger profilingLogger,
        IUmbracoVersion umbracoVersion,
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtimeState,
        PackageMigrationRunner packageMigrationRunner,
        IOptions<UnattendedSettings> unattendedSettings)
        : this(
            profilingLogger,
            umbracoVersion,
            databaseBuilder,
            runtimeState,
            packageMigrationRunner,
            unattendedSettings,
            StaticServiceProvider.Instance.GetRequiredService<DistributedCache>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogger<UnattendedUpgrader>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnattendedUpgrader"/> class, responsible for performing unattended upgrades of the Umbraco database and executing package migrations.
    /// </summary>
    /// <param name="profilingLogger">The logger used for profiling and diagnostic logging during the upgrade process.</param>
    /// <param name="umbracoVersion">Provides information about the current Umbraco version.</param>
    /// <param name="databaseBuilder">Handles database schema creation and upgrades.</param>
    /// <param name="runtimeState">Represents the current runtime state of the Umbraco application.</param>
    /// <param name="packageMigrationRunner">Executes package migrations as part of the upgrade process.</param>
    /// <param name="unattendedSettings">The configuration options for unattended upgrades.</param>
    /// <param name="distributedCache">Manages distributed cache invalidation during the upgrade.</param>
    /// <param name="logger">The logger instance for logging upgrade operations and errors.</param>
    public UnattendedUpgrader(
        IProfilingLogger profilingLogger,
        IUmbracoVersion umbracoVersion,
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtimeState,
        PackageMigrationRunner packageMigrationRunner,
        IOptions<UnattendedSettings> unattendedSettings,
        DistributedCache distributedCache,
        ILogger<UnattendedUpgrader> logger)
    {
        _profilingLogger = profilingLogger;
        _umbracoVersion = umbracoVersion;
        _databaseBuilder = databaseBuilder;
        _runtimeState = runtimeState;
        _packageMigrationRunner = packageMigrationRunner;
        _unattendedSettings = unattendedSettings.Value;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously handles the unattended upgrade process for the application, executing core and package migrations as required based on the provided notification.
    /// </summary>
    /// <param name="notification">The notification containing information about the runtime unattended upgrade, including upgrade results and migration status.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the runtime state reason is not recognized as a valid upgrade scenario.</exception>
    public async Task HandleAsync(RuntimeUnattendedUpgradeNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.RunUnattendedBootLogic())
        {
            switch (_runtimeState.Reason)
            {
                case RuntimeLevelReason.UpgradeMigrations:
                {
                    await RunUpgradeAsync(notification);

                    // If we errored out when upgrading don't do anything.
                    if (notification.UnattendedUpgradeResult is RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors)
                    {
                        return;
                    }

                    // It's entirely possible that there's both a core upgrade and package migrations to run, so try and run package migrations too.
                    // but only if upgrade unattended is enabled.
                    if (_unattendedSettings.PackageMigrationsUnattended)
                    {
                        await RunPackageMigrationsAsync(notification);
                    }
                }

                break;
                case RuntimeLevelReason.UpgradePackageMigrations:
                {
                    await RunPackageMigrationsAsync(notification);
                }

                break;
                default:
                throw new InvalidOperationException("Invalid reason " + _runtimeState.Reason);
            }
        }
    }

    private async Task RunPackageMigrationsAsync(RuntimeUnattendedUpgradeNotification notification)
    {
        if (_runtimeState.StartupState.TryGetValue(
                RuntimeState.PendingPackageMigrationsStateKey,
                out var pm) is false
            || pm is not IReadOnlyList<string> pendingMigrations)
        {
            throw new InvalidOperationException(
                $"The required key {RuntimeState.PendingPackageMigrationsStateKey} does not exist in startup state");
        }

        if (pendingMigrations.Count == 0)
        {
            // If we determined we needed to run package migrations but there are none, this is an error
            if (_runtimeState.Reason is RuntimeLevelReason.UpgradePackageMigrations)
            {
                throw new InvalidOperationException(
                    "No pending migrations found but the runtime level reason is " +
                    RuntimeLevelReason.UpgradePackageMigrations);
            }

            return;
        }

        try
        {
            await _packageMigrationRunner.RunPackagePlansAsync(pendingMigrations);
            notification.UnattendedUpgradeResult = RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete;

            // Migration plans may have changed published content, so refresh the distributed cache to ensure consistency on first request.
            _distributedCache.RefreshAllPublishedSnapshot();
            _logger.LogInformation(
                "Migration plans run: {Plans}. Triggered refresh of distributed published content cache.",
                string.Join(", ", pendingMigrations));
        }
        catch (Exception ex)
        {
            SetRuntimeError(ex);
            notification.UnattendedUpgradeResult =
                RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors;
        }
    }

    private async Task RunUpgradeAsync(RuntimeUnattendedUpgradeNotification notification)
    {
        var plan = new UmbracoPlan(_umbracoVersion);
        using (!_profilingLogger.IsEnabled(Core.Logging.LogLevel.Verbose) ? null : _profilingLogger.TraceDuration<UnattendedUpgrader>(
                   "Starting unattended upgrade.",
                   "Unattended upgrade completed."))
        {
            DatabaseBuilder.Result? result = await _databaseBuilder.UpgradeSchemaAndDataAsync(plan);
            if (result?.Success == false)
            {
                var innerException = new UnattendedInstallException(
                    "An error occurred while running the unattended upgrade.\n" + result.Message);
                _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, innerException);
            }

            notification.UnattendedUpgradeResult =
                RuntimeUnattendedUpgradeNotification.UpgradeResult.CoreUpgradeComplete;
        }
    }

    private void SetRuntimeError(Exception exception)
        => _runtimeState.Configure(
            RuntimeLevel.BootFailed,
            RuntimeLevelReason.BootFailedOnException,
            exception);
}
