using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

/// <summary>
/// Step responsible for configuring the database during Umbraco installation.
/// </summary>
public class DatabaseConfigureStep : StepBase, IInstallStep
{
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly ILogger<DatabaseConfigureStep> _logger;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Installer.Steps.DatabaseConfigureStep"/> class.
    /// </summary>
    /// <param name="databaseBuilder">The <see cref="DatabaseBuilder"/> responsible for configuring and initializing the database schema.</param>
    /// <param name="connectionStrings">The <see cref="IOptionsMonitor{T}"/> providing access to the application's connection strings.</param>
    /// <param name="logger">The <see cref="ILogger{DatabaseConfigureStep}"/> used for logging diagnostic and operational information.</param>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> used for mapping between different object models within Umbraco.</param>
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

    /// <summary>
    /// Executes the database configuration step using the provided installation data.
    /// </summary>
    /// <param name="model">The installation data containing database configuration details.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an attempt indicating success or failure of the installation result.</returns>
    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData model)
    {
        DatabaseModel databaseModel = _mapper.Map<DatabaseModel>(model.Database)!;

        if (!_databaseBuilder.ConfigureDatabaseConnection(databaseModel, false))
        {
            return Task.FromResult(FailWithMessage("Could not connect to the database"));
        }

        return Task.FromResult(Success());
    }

    /// <summary>
    /// Determines whether the database configuration step requires execution based on the current connection string configuration.
    /// </summary>
    /// <param name="model">The installation data model.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the step requires execution; otherwise, false.</returns>
    public Task<bool> RequiresExecutionAsync(InstallData model)
    {
        // If the connection string is already present in config we don't need to configure it again
        return Task.FromResult(_connectionStrings.CurrentValue.IsConnectionStringConfigured() is false);
    }
}
