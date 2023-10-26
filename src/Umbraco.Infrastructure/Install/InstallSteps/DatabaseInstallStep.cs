using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Install.InstallSteps;

[Obsolete("Will be replace with a new step with the new backoffice")]
[InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade, "DatabaseInstall", 11, "")]
public class DatabaseInstallStep : InstallSetupStep<object>
{
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IRuntimeState _runtime;

    public DatabaseInstallStep(IRuntimeState runtime, DatabaseBuilder databaseBuilder)
    {
        _runtime = runtime;
        _databaseBuilder = databaseBuilder;
    }

    public override Task<InstallSetupResult?> ExecuteAsync(object model)
    {
        if (_runtime.Level == RuntimeLevel.Run)
        {
            throw new Exception("Umbraco is already configured!");
        }

        if (_runtime.Reason == RuntimeLevelReason.InstallMissingDatabase)
        {
            _databaseBuilder.CreateDatabase();
        }

        // Retry every 500ms for up to 10 times before failing.
        // Typically, if being ran after CreateDatabase() it can take a few moments for the DB to become available.
        DatabaseBuilder.Result? result = TryCreateSchemaAndData();

        if (result?.Success == false)
        {
            throw new InstallException("The database failed to install. ERROR: " + result.Message);
        }

        if (result?.RequiresUpgrade == false)
        {
            return Task.FromResult<InstallSetupResult?>(null);
        }

        // Upgrade is required, so set the flag for the next step
        return Task.FromResult(new InstallSetupResult(new Dictionary<string, object> { { "upgrade", true } }))!;
    }

    public override bool RequiresExecution(object model) => true;

    private DatabaseBuilder.Result? TryCreateSchemaAndData()
    {
        int maxRetries = 10;
        int retryCount = 0;
        DatabaseBuilder.Result? result = null;

        while (retryCount < maxRetries)
        {
            result = _databaseBuilder.CreateSchemaAndData();

            if (result?.Success == true)
            {
                return result;
            }

            retryCount++;
            Thread.Sleep(500); // 0.5 second delay between retries
        }

        return result;
    }
}
