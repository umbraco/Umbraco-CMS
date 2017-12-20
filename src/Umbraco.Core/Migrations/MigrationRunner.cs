using System;
using System.Collections.Generic;
using System.Linq;
using Semver;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Represents the Migration Runner, which is used to apply migrations to
    /// the umbraco database.
    /// </summary>
    public class MigrationRunner
    {
        private readonly IMigrationCollectionBuilder _builder;
        private readonly IMigrationEntryService _migrationEntryService;
        private readonly ILogger _logger;
        private readonly SemVersion _currentVersion;
        private readonly SemVersion _targetVersion;
        private readonly string _productName;
        private readonly IMigration[] _migrations;

        public MigrationRunner(IMigrationCollectionBuilder builder, IMigrationEntryService migrationEntryService, ILogger logger, SemVersion currentVersion, SemVersion targetVersion, string productName, params IMigration[] migrations)
        {
            if (string.IsNullOrWhiteSpace(productName)) throw new ArgumentNullOrEmptyException(nameof(productName));

            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _migrationEntryService = migrationEntryService ?? throw new ArgumentNullException(nameof(migrationEntryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            _targetVersion = targetVersion ?? throw new ArgumentNullException(nameof(targetVersion));
            _productName = productName;
            //ensure this is null if there aren't any
            _migrations = migrations == null || migrations.Length == 0 ? null : migrations;
        }

        /// <summary>
        /// Executes the migrations against the database.
        /// </summary>
        /// <param name="migrationContext">The migration context to execute migrations with</param>
        /// <param name="isUpgrade">Boolean indicating whether this is an upgrade or downgrade</param>
        /// <returns><c>True</c> if migrations were applied, otherwise <c>False</c></returns>
        public bool Execute(IMigrationContext migrationContext, bool isUpgrade = true)
        {
            _logger.Info<MigrationRunner>("Initializing database migrations");

            var foundMigrations = FindMigrations(migrationContext);

            //filter all non-schema migrations
            var migrations = isUpgrade
                                 ? OrderedUpgradeMigrations(foundMigrations).ToList()
                                 : OrderedDowngradeMigrations(foundMigrations).ToList();


            if (Migrating.IsRaisedEventCancelled(new MigrationEventArgs(migrations, _currentVersion, _targetVersion, _productName, true), this))
            {
                _logger.Warn<MigrationRunner>("Migration was cancelled by an event");
                return false;
            }

            try
            {
                ExecuteMigrations(migrationContext, migrations, isUpgrade);
            }
            catch (Exception ex)
            {
                //if this fails then the transaction will be rolled back, BUT if we are using MySql this is not the case,
                //since it does not support schema changes in a transaction, see: http://dev.mysql.com/doc/refman/5.0/en/implicit-commit.html
                //so in that case we have to downgrade
                if (migrationContext.Database.DatabaseType is NPoco.DatabaseTypes.MySqlDatabaseType)
                {
                    throw new DataLossException(
                            "An error occurred running a schema migration but the changes could not be rolled back. Error: " + ex.Message + ". In some cases, it may be required that the database be restored to it's original state before running this upgrade process again.",
                            ex);
                }

                //continue throwing the exception
                throw;
            }

            Migrated.RaiseEvent(new MigrationEventArgs(migrations, migrationContext, _currentVersion, _targetVersion, _productName, false), this);

            return true;
        }

        /// <summary>
        /// Filters and orders migrations based on the migrations listed and the currently configured version and the target installation version
        /// </summary>
        /// <param name="foundMigrations"></param>
        /// <returns></returns>
        public IEnumerable<IMigration> OrderedUpgradeMigrations(IEnumerable<IMigration> foundMigrations)
        {
            //get the version instance to compare with the migrations, this will be a normal c# Version object with only 3 parts
            var targetVersionToCompare = _targetVersion.GetVersion(3);
            var currentVersionToCompare = _currentVersion.GetVersion(3);

            var migrations = (from migration in foundMigrations
                let migrationAttributes = migration.GetType().GetCustomAttributes<MigrationAttribute>(false)
                from migrationAttribute in migrationAttributes
                where migrationAttribute != null
                where migrationAttribute.TargetVersion > currentVersionToCompare &&
                      migrationAttribute.TargetVersion <= targetVersionToCompare &&
                      migrationAttribute.ProductName == _productName &&
                      //filter if the migration specifies a minimum current version for which to execute
                      (migrationAttribute.MinimumCurrentVersion == null || currentVersionToCompare >= migrationAttribute.MinimumCurrentVersion)
                orderby migrationAttribute.TargetVersion, migrationAttribute.SortOrder ascending
                select migration).Distinct();
            return migrations;
        }

        /// <summary>
        /// Filters and orders migrations based on the migrations listed and the currently configured version and the target installation version
        /// </summary>
        /// <param name="foundMigrations"></param>
        /// <returns></returns>
        public IEnumerable<IMigration> OrderedDowngradeMigrations(IEnumerable<IMigration> foundMigrations)
        {
            //get the version instance to compare with the migrations, this will be a normal c# Version object with only 3 parts
            var targetVersionToCompare = _targetVersion.GetVersion(3);
            var currentVersionToCompare = _currentVersion.GetVersion(3);

            var migrations = (from migration in foundMigrations
                let migrationAttributes = migration.GetType().GetCustomAttributes<MigrationAttribute>(false)
                from migrationAttribute in migrationAttributes
                where migrationAttribute != null
                where
                    migrationAttribute.TargetVersion > currentVersionToCompare &&
                    migrationAttribute.TargetVersion <= targetVersionToCompare &&
                    migrationAttribute.ProductName == _productName &&
                    //filter if the migration specifies a minimum current version for which to execute
                    (migrationAttribute.MinimumCurrentVersion == null || currentVersionToCompare >= migrationAttribute.MinimumCurrentVersion)
                orderby migrationAttribute.TargetVersion, migrationAttribute.SortOrder descending
                select migration).Distinct();
            return migrations;
        }

        /// <summary>
        /// Find all migrations that are available through the <see cref="MigrationResolver"/>
        /// </summary>
        /// <returns>An array of <see cref="IMigration"/></returns>
        protected IMigration[] FindMigrations(IMigrationContext context)
        {
            //MCH NOTE: Consider adding the ProductName filter to the Resolver so we don't get a bunch of irrelevant migrations
            return _migrations ?? _builder.CreateCollection(context).ToArray();
        }

        internal void ExecuteMigrations(IMigrationContext context, IEnumerable<IMigration> migrations, bool isUp = true)
        {
            using (var transaction = context.Database.GetTransaction()) // fixme scope?
            {
                foreach (var migration in migrations)
                {
                    if (isUp) migration.Up();
                    else migration.Down();
                }

                var exists = _migrationEntryService.FindEntry(_productName, _targetVersion); // fixme refactor
                if (exists == null)
                    _migrationEntryService.CreateEntry(_productName, _targetVersion);

                transaction.Complete();
            }
        }

        /// <summary>
        /// Occurs before Migration
        /// </summary>
        public static event TypedEventHandler<MigrationRunner, MigrationEventArgs> Migrating;

        /// <summary>
        /// Occurs after Migration
        /// </summary>
        public static event TypedEventHandler<MigrationRunner, MigrationEventArgs> Migrated;
    }
}
