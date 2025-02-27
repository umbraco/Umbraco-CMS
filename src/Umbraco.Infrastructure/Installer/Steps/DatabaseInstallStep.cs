using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

public class DatabaseInstallStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly IRuntimeState _runtime;
    private readonly DatabaseBuilder _databaseBuilder;

    public DatabaseInstallStep(IRuntimeState runtime, DatabaseBuilder databaseBuilder)
    {
        _runtime = runtime;
        _databaseBuilder = databaseBuilder;
    }

    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

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

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private Task<bool> ShouldExecute()
        => Task.FromResult(true);
}
