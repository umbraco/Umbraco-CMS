using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

public class DatabaseConfigureStep : IInstallStep
{
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly ILogger<DatabaseConfigureStep> _logger;
    private readonly IUmbracoMapper _mapper;

    public DatabaseConfigureStep(
        DatabaseBuilder databaseBuilder,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        ILogger<DatabaseConfigureStep> logger,
        IUmbracoMapper mapper)
    {
        _databaseBuilder = databaseBuilder;
        _connectionStrings = connectionStrings;
        _logger = logger;
        _mapper = mapper;
    }

    public Task ExecuteAsync(InstallData model)
    {
        DatabaseModel databaseModel = _mapper.Map<DatabaseModel>(model.Database)!;

        if (!_databaseBuilder.ConfigureDatabaseConnection(databaseModel, false))
        {
            throw new InstallException("Could not connect to the database");
        }

        return Task.CompletedTask;
    }

    public Task<bool> RequiresExecutionAsync(InstallData model)
    {
        // If the connection string is already present in config we don't need to configure it again
        if (_connectionStrings.CurrentValue.IsConnectionStringConfigured())
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
