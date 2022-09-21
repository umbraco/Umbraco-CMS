using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install.InstallSteps;

[Obsolete("Will be replace with a new step with the new backoffice")]
[InstallSetupStep(InstallationType.NewInstall, "DatabaseConfigure", "database", 10, "Setting up a database, so Umbraco has a place to store your website", PerformsAppRestart = true)]
public class DatabaseConfigureStep : InstallSetupStep<DatabaseModel>
{
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata;
    private readonly ILogger<DatabaseConfigureStep> _logger;

    public DatabaseConfigureStep(
        DatabaseBuilder databaseBuilder,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        ILogger<DatabaseConfigureStep> logger,
        IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata)
    {
        _databaseBuilder = databaseBuilder;
        _connectionStrings = connectionStrings;
        _logger = logger;
        _databaseProviderMetadata = databaseProviderMetadata;
    }

    public override object ViewModel => new {databases = _databaseProviderMetadata.GetAvailable().ToList()};

    public override string View => ShouldDisplayView() ? base.View : string.Empty;

    public override Task<InstallSetupResult?> ExecuteAsync(DatabaseModel databaseSettings)
    {
        if (!_databaseBuilder.ConfigureDatabaseConnection(databaseSettings, false))
        {
            throw new InstallException("Could not connect to the database");
        }

        return Task.FromResult<InstallSetupResult?>(null);
    }

    public override bool RequiresExecution(DatabaseModel model) => ShouldDisplayView();

    private bool ShouldDisplayView()
    {
        // If the connection string is already present in config we don't need to show the settings page and we jump to installing/upgrading.
        if (_connectionStrings.CurrentValue.IsConnectionStringConfigured())
        {
            try
            {
                // Since a connection string was present we verify the db can connect and query
                _databaseBuilder.ValidateSchema();

                return false;
            }
            catch (Exception ex)
            {
                // Something went wrong, could not connect so probably need to reconfigure
                _logger.LogError(ex, "An error occurred, reconfiguring...");

                return true;
            }
        }

        return true;
    }
}
