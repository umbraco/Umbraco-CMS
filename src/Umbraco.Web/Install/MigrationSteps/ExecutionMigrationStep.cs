using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.MigrationSteps
{
    [InstallSetupStep("Execute Migrations", 2, "", 
        //This isn't always the case but packages can certainly restart during migrations
        PerformsAppRestart = true)]
    internal class ExecutionMigrationStep : InstallSetupStep<object>
    {
        private readonly IMigrationEntryService _migrationEntryService;
        private readonly PackageMigrationsContext _packageMigrationsContext;
        private readonly Database _database;
        private readonly DatabaseProviders _databaseProvider;
        private readonly ILogger _logger;

        public ExecutionMigrationStep(ApplicationContext applicationContext)
        {
            _migrationEntryService = applicationContext.Services.MigrationEntryService;
            _packageMigrationsContext = applicationContext.PackageMigrationsContext;
            _database = applicationContext.DatabaseContext.Database;
            _databaseProvider = applicationContext.DatabaseContext.DatabaseProvider;
            _logger = applicationContext.ProfilingLogger.Logger;
        }

        public ExecutionMigrationStep(IMigrationEntryService migrationEntryService,
            PackageMigrationsContext packageMigrationsContext, Database database, 
            DatabaseProviders databaseProvider, ILogger logger)
        {
            _migrationEntryService = migrationEntryService;
            _packageMigrationsContext = packageMigrationsContext;
            _database = database;
            _databaseProvider = databaseProvider;
            _logger = logger;
        }

        public override bool RequiresExecution(object model)
        {
            return _packageMigrationsContext.HasPendingPackageMigrations;
        }        

        public override InstallSetupResult Execute(object model)
        {
            //NOTE: We could separate these out into separate tasks (dynamically) if we wanted to but I 
            // think it will be fine processing them all at once.
            var pendingMigrations = _packageMigrationsContext.GetPendingPackageMigrations();
            var packageDbMigrations = _migrationEntryService
                .FindEntries(pendingMigrations.Select(x => x.Key))
                .ToArray();

            foreach (var pendingPackageMigration in _packageMigrationsContext.GetPendingPackageMigrations())
            {
                var latestDbMigration = packageDbMigrations
                    .Where(x => x.MigrationName.InvariantEquals(pendingPackageMigration.Key))
                    .OrderByDescending(x => x.Version)
                    .FirstOrDefault();

                var runner = new MigrationRunner(_migrationEntryService, _logger,
                    //This is the version this package is coming from:
                    latestDbMigration == null ? new SemVersion(0) : latestDbMigration.Version,
                    //This is the latest migration specified in the packages c# migrations
                    pendingPackageMigration.Value, pendingPackageMigration.Key);

                var result = runner.Execute(_database, _databaseProvider, true);
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
            _packageMigrationsContext.ResetPendingPackageMigrations();

            //everything was ok
            return null;
        }
    }
}