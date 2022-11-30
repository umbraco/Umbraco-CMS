using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Infrastructure.Installer.Steps;

public class DatabaseInstallStep : IInstallStep, IUpgradeStep
{
    private readonly IRuntimeState _runtime;
    private readonly DatabaseBuilder _databaseBuilder;

    public DatabaseInstallStep(IRuntimeState runtime, DatabaseBuilder databaseBuilder)
    {
        _runtime = runtime;
        _databaseBuilder = databaseBuilder;
    }

    public Task ExecuteAsync(InstallData _) => Execute();

    public Task ExecuteAsync() => Execute();

    private Task Execute()
    {

        if (_runtime.Reason == RuntimeLevelReason.InstallMissingDatabase)
        {
            _databaseBuilder.CreateDatabase();
        }

        DatabaseBuilder.Result? result = _databaseBuilder.CreateSchemaAndData();

        if (result?.Success == false)
        {
            throw new InstallException("The database failed to install. ERROR: " + result.Message);
        }

        return Task.CompletedTask;
    }

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private Task<bool> ShouldExecute()
        => Task.FromResult(true);
}
