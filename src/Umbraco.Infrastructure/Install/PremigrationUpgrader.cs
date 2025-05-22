using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
///     Handles <see cref="RuntimePremigrationsUpgradeNotification" /> to execute the unattended Umbraco upgrader
///     or the unattended Package migrations runner.
/// </summary>
public class PremigrationUpgrader : INotificationAsyncHandler<RuntimePremigrationsUpgradeNotification>
{
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IRuntimeState _runtimeState;
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IKeyValueService _keyValueService;

    public PremigrationUpgrader(
        IProfilingLogger profilingLogger,
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtimeState,
        IUmbracoDatabaseFactory umbracoDatabaseFactory,
        IKeyValueService keyValueService)
    {
        _profilingLogger = profilingLogger ?? throw new ArgumentNullException(nameof(profilingLogger));
        _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
        _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _keyValueService = keyValueService;
    }

    public async Task HandleAsync(RuntimePremigrationsUpgradeNotification notification, CancellationToken cancellationToken)
    {
        // no connection string set
        if (_umbracoDatabaseFactory.Configured is false)
        {
            return;
        }

        if (_databaseBuilder.IsUmbracoInstalled() is false)
        {
            return;
        }

        var plan = new UmbracoPremigrationPlan();
        if (HasMissingPremigrations(plan) is false)
        {
            return;
        }

        using (_profilingLogger.IsEnabled(LogLevel.Verbose) is false ? null : _profilingLogger.TraceDuration<UnattendedUpgrader>("Starting premigration upgrade.", "Unattended premigration completed."))
        {
            DatabaseBuilder.Result? result = await _databaseBuilder.UpgradeSchemaAndDataAsync(plan).ConfigureAwait(false);
            if (result?.Success is false)
            {
                var innerException = new IOException("An error occurred while running the premigration upgrade.\n" + result.Message);
                _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, innerException);
            }

            notification.UpgradeResult = RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.CoreUpgradeComplete;
        }
    }

    private bool HasMissingPremigrations(UmbracoPremigrationPlan umbracoPremigrationPlan)
    {
        var premigrationState = _keyValueService.GetValue(Constants.Conventions.Migrations.UmbracoUpgradePlanPremigrationsKey);

        return umbracoPremigrationPlan.FinalState != premigrationState;
    }
}
