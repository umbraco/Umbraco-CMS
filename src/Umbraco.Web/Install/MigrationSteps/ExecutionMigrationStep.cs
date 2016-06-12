using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.MigrationSteps
{
    [InstallSetupStep("Execute Migrations", 2, "", 
        //This isn't always the case but packages can certainly restart during migrations
        PerformsAppRestart = true)]
    internal class ExecutionMigrationStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;

        public ExecutionMigrationStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override bool RequiresExecution(object model)
        {
            return _applicationContext.PackageMigrationsContext.HasPendingPackageMigrations;
        }        

        public override InstallSetupResult Execute(object model)
        {
            //NOTE: We could separate these out into separate tasks (dynamically) if we wanted to but I 
            // think it will be fine processing them all at once.

            var pendingMigrations = _applicationContext.PackageMigrationsContext.GetPendingPackageMigrations();
            var packageDbMigrations = _applicationContext.Services.MigrationEntryService
                .FindEntries(pendingMigrations.Select(x => x.Key))
                .ToArray();

            foreach (var pendingPackageMigration in _applicationContext.PackageMigrationsContext.GetPendingPackageMigrations())
            {
                var latestDbMigration = packageDbMigrations
                    .Where(x => x.MigrationName.InvariantEquals(pendingPackageMigration.Key))
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefault();

                var runner = new MigrationRunner(_applicationContext.Services.MigrationEntryService,
                    _applicationContext.ProfilingLogger.Logger,
                    //This is the version this package is coming from:
                    latestDbMigration == null ? new SemVersion(0) : latestDbMigration.Version,
                    //This is the latest migration specified in the packages c# migrations
                    pendingPackageMigration.Value,
                    pendingPackageMigration.Key);

                var result = runner.Execute(_applicationContext.DatabaseContext.Database, _applicationContext.DatabaseContext.DatabaseProvider, true);

                if (result == false)
                {
                    //this will only happen normally if something cancels the migration
                    return new InstallSetupResult("failedmigration", new
                    {
                        productName = pendingPackageMigration
                    });
                }
            }
            
            //very important to reset migrations since an app restart isn't actually a mandatory thing
            _applicationContext.PackageMigrationsContext.ResetPendingPackageMigrations();

            //everything was ok
            return null;
        }
    }
}