using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Install;

public class UnattendedInstaller : INotificationAsyncHandler<RuntimeUnattendedInstallNotification>
{
    private readonly IUmbracoDatabaseFactory _databaseFactory;

    private readonly IDatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
    private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<UnattendedInstaller> _logger;
    private readonly IRuntimeState _runtimeState;
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IOptions<UnattendedSettings> _unattendedSettings;

    public UnattendedInstaller(
        IDatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        IEventAggregator eventAggregator,
        IOptions<UnattendedSettings> unattendedSettings,
        IUmbracoDatabaseFactory databaseFactory,
        IDbProviderFactoryCreator dbProviderFactoryCreator,
        ILogger<UnattendedInstaller> logger,
        IRuntimeState runtimeState,
        DatabaseBuilder databaseBuilder)
    {
        _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ??
                                        throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        _unattendedSettings = unattendedSettings;
        _databaseFactory = databaseFactory;
        _dbProviderFactoryCreator = dbProviderFactoryCreator;
        _logger = logger;
        _runtimeState = runtimeState;
        _databaseBuilder = databaseBuilder;
    }

    public Task HandleAsync(RuntimeUnattendedInstallNotification notification, CancellationToken cancellationToken)
    {
        // unattended install is not enabled
        if (_unattendedSettings.Value.InstallUnattended == false)
        {
            return Task.CompletedTask;
        }

        // no connection string set
        if (_databaseFactory.Configured == false)
        {
            return Task.CompletedTask;
        }


        _runtimeState.DetermineRuntimeLevel();
        if (_runtimeState.Reason == RuntimeLevelReason.InstallMissingDatabase)
        {
            _databaseBuilder.CreateDatabase();
        }

        try
        {
            if (_databaseBuilder.IsUmbracoInstalled())
            {
                return Task.CompletedTask;
            }

            _logger.LogInformation("Starting unattended install.");
            _databaseBuilder.CreateSchemaAndData();
            _logger.LogInformation("Unattended install completed.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Error during unattended install.");

            var innerException = new UnattendedInstallException(
                "The database configuration failed."
                + "\n Please check log file for additional information (can be found in '/Umbraco/Data/Logs/')",
                ex);

            _runtimeState.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, innerException);
        }

        _eventAggregator.Publish(new UnattendedInstallNotification());


        return Task.CompletedTask;
    }
}
