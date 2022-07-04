using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.New.Cms.Core.Installer.Steps;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Infrastructure.Installer.Steps;

public class NewDatabaseInstallStep : NewInstallSetupStep
{

    private readonly IRuntimeState _runtime;
    private readonly DatabaseBuilder _databaseBuilder;

    public NewDatabaseInstallStep(IRuntimeState runtime, DatabaseBuilder databaseBuilder)
        : base(
            "DatabaseInstall",
            40,
            InstallationType.NewInstall | InstallationType.Upgrade)
    {
        _runtime = runtime;
        _databaseBuilder = databaseBuilder;
    }

    public override Task ExecuteAsync(InstallData model)
    {
        if (_runtime.Level == RuntimeLevel.Run)
        {
            throw new Exception("Umbraco is already configured!");
        }

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

    public override Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(true);
}
