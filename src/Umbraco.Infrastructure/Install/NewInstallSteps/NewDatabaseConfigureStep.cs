using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Install.NewInstallSteps;
using Umbraco.Cms.Core.Install.NewModels;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install.NewInstallSteps;

public class NewDatabaseConfigureStep : NewInstallSetupStep
{
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly ILogger<NewDatabaseConfigureStep> _logger;
    private readonly IUmbracoMapper _mapper;

    public NewDatabaseConfigureStep(
        DatabaseBuilder databaseBuilder,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        ILogger<NewDatabaseConfigureStep> logger,
        IUmbracoMapper mapper)
        : base(
            "DatabaseConfigure",
            30,
            InstallationType.NewInstall)
    {
        _databaseBuilder = databaseBuilder;
        _connectionStrings = connectionStrings;
        _logger = logger;
        _mapper = mapper;
    }

    public override Task ExecuteAsync(InstallData model)
    {
        DatabaseModel databaseModel = _mapper.Map<DatabaseModel>(model.Database)!;

        if (!_databaseBuilder.ConfigureDatabaseConnection(databaseModel, false))
        {
            throw new InstallException("Could not connect to the database");
        }

        return Task.FromResult<InstallSetupResult?>(null);
    }

    public override Task<bool> RequiresExecutionAsync(InstallData model)
    {
        // If the connection string is already present in config we don't need to show the settings page and we jump to installing/upgrading.
        if (_connectionStrings.CurrentValue.IsConnectionStringConfigured())
        {
            try
            {
                // Since a connection string was present we verify the db can connect and query
                _databaseBuilder.ValidateSchema();

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                // Something went wrong, could not connect so probably need to reconfigure
                _logger.LogError(ex, "An error occurred, reconfiguring...");

                return Task.FromResult(true);
            }
        }

        return Task.FromResult(true);
    }
}
