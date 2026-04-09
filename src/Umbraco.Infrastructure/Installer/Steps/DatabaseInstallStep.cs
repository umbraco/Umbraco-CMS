using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

/// <summary>
/// Represents the installation step in the Umbraco installer that is responsible for configuring and initializing the database.
/// </summary>
public class DatabaseInstallStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly IRuntimeState _runtime;
    private readonly DatabaseBuilder _databaseBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Installer.Steps.DatabaseInstallStep"/> class.
    /// </summary>
    /// <param name="runtime">The current runtime state of the Umbraco application.</param>
    /// <param name="databaseBuilder">The builder responsible for creating and upgrading the database schema.</param>
    public DatabaseInstallStep(IRuntimeState runtime, DatabaseBuilder databaseBuilder)
    {
        _runtime = runtime;
        _databaseBuilder = databaseBuilder;
    }

    /// <summary>
    /// Executes the database installation step asynchronously.
    /// </summary>
    /// <param name="_">The installation data. (Unused parameter)</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{T}"/> with the installation result.</returns>
    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    /// <summary>
    /// Asynchronously executes the database installation step.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing an <see cref="Attempt{InstallationResult}"/> that indicates the success or failure of the installation.</returns>
    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    private Task<Attempt<InstallationResult>> Execute()
    {

        if (_runtime.Reason == RuntimeLevelReason.InstallMissingDatabase)
        {
            _databaseBuilder.CreateDatabase();
        }

        DatabaseBuilder.Result? result = _databaseBuilder.CreateSchemaAndData();

        if (result?.Success == false)
        {
            return Task.FromResult(FailWithMessage("The database failed to install. ERROR: " + result.Message));
        }

        return Task.FromResult(Success());
    }

    /// <summary>
    /// Asynchronously determines whether this database installation step needs to be executed.
    /// </summary>
    /// <param name="_">Unused. Installation data parameter (not used in this implementation).</param>
    /// <returns>A task that returns <c>true</c> if execution is required; otherwise, <c>false</c>.</returns>
    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    /// <summary>
    /// Determines asynchronously whether the database installation step requires execution.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if execution is required; otherwise, false.</returns>
    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private Task<bool> ShouldExecute()
        => Task.FromResult(true);
}
