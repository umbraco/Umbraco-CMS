using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal class PackageMigrationsContext
    {
        private readonly DatabaseContext _dbContext;
        private readonly IMigrationEntryService _migrationEntryService;
        private readonly ILogger _logger;

        public PackageMigrationsContext(DatabaseContext dbContext, IMigrationEntryService migrationEntryService, ILogger logger)
        {
            _dbContext = dbContext;
            _migrationEntryService = migrationEntryService;
            _logger = logger;
            ResetPendingPackageMigrations();
        }

        //List of product names and their maximum sem version registered in migrations
        private Lazy<ReadOnlyDictionary<string, SemVersion>> _packageVersionsConfigured;

        /// <summary>
        /// Called to initialize or when package migrations have executed - since an app restart is mandatory
        /// </summary>
        internal void ResetPendingPackageMigrations()
        {
            _packageVersionsConfigured = new Lazy<ReadOnlyDictionary<string, SemVersion>>(() =>
            {
                var result = new Dictionary<string, SemVersion>();

                //The versions are the same in config, but are they the same in the database. We can only check this
                // if we have a db context available, if we don't then we are not installed anyways
                if (_dbContext.IsDatabaseConfigured && _dbContext.CanConnect)
                {
                    //Now we can check if there are any package migrations that haven't been executed.

                    //Find and group the packages that have migrations.
                    //Then we need to determine if there are outstanding migrations that need to be run based on the
                    // migration versions stored in the db compared to the available migrations.
                    var packageMigrations = MigrationResolver.Current.MigrationMetaData
                        .Where(x => x.ProductName != GlobalSettings.UmbracoMigrationName)
                        .GroupBy(x => x.ProductName)
                        .ToArray();

                    var packageMigrationProductNames = packageMigrations.Select(x => x.Key).Distinct().ToArray();

                    var allPackageDbEntries = _migrationEntryService.FindEntries(packageMigrationProductNames)
                        .ToArray();

                    foreach (var packageMigration in packageMigrations)
                    {
                        var packageDbVersions = allPackageDbEntries
                            .Where(x => x.MigrationName.InvariantEquals(packageMigration.Key))
                            .Select(x => x.Version);

                        var notExecuted = packageMigration
                            .Select(x => x.TargetVersion.GetSemanticVersion())
                            .Except(packageDbVersions)
                            .ToArray();

                        if (notExecuted.Any())
                        {
                            var maxVersion = notExecuted.Max();
                            result.Add(packageMigration.Key, maxVersion);

                            _logger.Debug<ApplicationContext>(
                                    string.Format("The migration {0} for version: '{1} has not been executed, there is no record in the database",
                                    packageMigration.Key,
                                    maxVersion.ToSemanticString()));
                        }
                    }
                    
                }

                return new ReadOnlyDictionary<string, SemVersion>(result);
            });
        }

        /// <summary>
        /// Returns true if there are pending package migrations that need to be executed
        /// </summary>
        internal bool HasPendingPackageMigrations
        {
            get { return _packageVersionsConfigured.Value.Any(); }
        }

        /// <summary>
        /// Returns the list of package migration names that need to be executed
        /// </summary>
        internal ReadOnlyDictionary<string, SemVersion> GetPendingPackageMigrations()
        {
            return _packageVersionsConfigured.Value;
        }
    }
}