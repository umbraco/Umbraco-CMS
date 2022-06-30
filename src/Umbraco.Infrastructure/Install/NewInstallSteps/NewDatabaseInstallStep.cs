using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Install.NewModels;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Install.NewInstallSteps;

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

        // TODO: Handle this
        // For some reason it's this steps that decides if the upgrade step needs to run :( We don't want this, these steps should be isolated from each other
        if (result?.RequiresUpgrade == false)
        {
            // return Task.FromResult<InstallSetupResult?>(null);
            return Task.CompletedTask;
        }

        // Upgrade is required, so set the flag for the next step
        // return Task.FromResult(new InstallSetupResult(new Dictionary<string, object> { { "upgrade", true } }));
        return Task.CompletedTask;
    }

    public override Task<bool> RequiresExecution(InstallData model) => Task.FromResult(true);
}
