using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
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

    public UnattendedUpgrader(
        IProfilingLogger profilingLogger,
        IUmbracoVersion umbracoVersion,
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtimeState,
        PackageMigrationRunner packageMigrationRunner,
        IOptions<UnattendedSettings> unattendedSettings)
    {
        _profilingLogger = profilingLogger ?? throw new ArgumentNullException(nameof(profilingLogger));
        _umbracoVersion = umbracoVersion ?? throw new ArgumentNullException(nameof(umbracoVersion));
        _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
        _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
        _packageMigrationRunner = packageMigrationRunner;
        _unattendedSettings = unattendedSettings.Value;
    }

    [Obsolete("Use constructor that takes IOptions<UnattendedSettings>, this will be removed in V16")]
    public UnattendedUpgrader(
        IProfilingLogger profilingLogger,
        IUmbracoVersion umbracoVersion,
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtimeState,
        PackageMigrationRunner packageMigrationRunner)
    : this(
        profilingLogger,
        umbracoVersion,
        databaseBuilder,
        runtimeState,
        packageMigrationRunner,
        StaticServiceProvider.Instance.GetRequiredService<IOptions<UnattendedSettings>>())
    {
    }

    public Task HandleAsync(RuntimeUnattendedUpgradeNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.RunUnattendedBootLogic())
        {
            switch (_runtimeState.Reason)
            {
                case RuntimeLevelReason.UpgradeMigrations:
                {
                    RunUpgrade(notification);

                    // If we errored out when upgrading don't do anything.
                    if (notification.UnattendedUpgradeResult is RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors)
                    {
                        return Task.CompletedTask;
                    }

                    // It's entirely possible that there's both a core upgrade and package migrations to run, so try and run package migrations too.
                    // but only if upgrade unattended is enabled.
                    if (_unattendedSettings.PackageMigrationsUnattended)
                    {
                        RunPackageMigrations(notification);
                    }
                }

                break;
                case RuntimeLevelReason.UpgradePackageMigrations:
                {
                    RunPackageMigrations(notification);
                }

                break;
                default:
                throw new InvalidOperationException("Invalid reason " + _runtimeState.Reason);
            }
        }

        return Task.CompletedTask;
    }

    private void RunPackageMigrations(RuntimeUnattendedUpgradeNotification notification)
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
            _packageMigrationRunner.RunPackagePlans(pendingMigrations);
            notification.UnattendedUpgradeResult = RuntimeUnattendedUpgradeNotification.UpgradeResult
                .PackageMigrationComplete;
        }
        catch (Exception ex)
        {
            SetRuntimeError(ex);
            notification.UnattendedUpgradeResult =
                RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors;
        }
    }

    private void RunUpgrade(RuntimeUnattendedUpgradeNotification notification)
    {
        var plan = new UmbracoPlan(_umbracoVersion);
        using (!_profilingLogger.IsEnabled(Core.Logging.LogLevel.Verbose) ? null : _profilingLogger.TraceDuration<UnattendedUpgrader>(
                   "Starting unattended upgrade.",
                   "Unattended upgrade completed."))
        {
            DatabaseBuilder.Result? result = _databaseBuilder.UpgradeSchemaAndData(plan);
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
