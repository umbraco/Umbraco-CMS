using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Represents the Migration Runner, which is used to apply migrations to
    /// the umbraco database.
    /// </summary>
    public class MigrationRunner
    {
        private readonly ILogger _logger;
        private readonly Version _currentVersion;
        private readonly Version _targetVersion;
        private readonly string _productName;

        [Obsolete("Use the ctor that specifies all dependencies instead")]
        public MigrationRunner(Version currentVersion, Version targetVersion, string productName)
            : this(LoggerResolver.Current.Logger, currentVersion, targetVersion, productName)
        {
        }

        public MigrationRunner(ILogger logger, Version currentVersion, Version targetVersion, string productName)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (currentVersion == null) throw new ArgumentNullException("currentVersion");
            if (targetVersion == null) throw new ArgumentNullException("targetVersion");
            Mandate.ParameterNotNullOrEmpty(productName, "productName");

            _logger = logger;
            _currentVersion = currentVersion;
            _targetVersion = targetVersion;
            _productName = productName;
        }

        /// <summary>
        /// Executes the migrations against the database.
        /// </summary>
        /// <param name="database">The PetaPoco Database, which the migrations will be run against</param>
        /// <param name="isUpgrade">Boolean indicating whether this is an upgrade or downgrade</param>
        /// <returns><c>True</c> if migrations were applied, otherwise <c>False</c></returns>
        public virtual bool Execute(Database database, bool isUpgrade = true)
        {
            return Execute(database, database.GetDatabaseProvider(), isUpgrade);
        }

        /// <summary>
        /// Executes the migrations against the database.
        /// </summary>
        /// <param name="database">The PetaPoco Database, which the migrations will be run against</param>
        /// <param name="databaseProvider"></param>
        /// <param name="isUpgrade">Boolean indicating whether this is an upgrade or downgrade</param>
        /// <returns><c>True</c> if migrations were applied, otherwise <c>False</c></returns>
        public virtual bool Execute(Database database, DatabaseProviders databaseProvider, bool isUpgrade = true)
        {
            _logger.Info<MigrationRunner>("Initializing database migrations");

            var foundMigrations = FindMigrations();

            //filter all non-schema migrations
            var migrations = isUpgrade
                                 ? OrderedUpgradeMigrations(foundMigrations).ToList()
                                 : OrderedDowngradeMigrations(foundMigrations).ToList();

            //SD: Why do we want this?
            //MCH: Because extensibility ... Mostly relevant to package developers who needs to utilize this type of event to add or remove migrations from the list
            if (Migrating.IsRaisedEventCancelled(new MigrationEventArgs(migrations, _currentVersion, _targetVersion, true), this))
                return false;

            //Loop through migrations to generate sql
            var migrationContext = InitializeMigrations(migrations, database, databaseProvider, isUpgrade);

            try
            {
                ExecuteMigrations(migrationContext, database);
            }
            catch (Exception ex)
            {
                //if this fails then the transaction will be rolled back, BUT if we are using MySql this is not the case,
                //since it does not support schema changes in a transaction, see: http://dev.mysql.com/doc/refman/5.0/en/implicit-commit.html
                //so in that case we have to downgrade
                if (databaseProvider == DatabaseProviders.MySql)
                {
                    throw new DataLossException(
                            "An error occurred running a schema migration but the changes could not be rolled back. Error: " + ex.Message + ". In some cases, it may be required that the database be restored to it's original state before running this upgrade process again.",
                            ex);
                }

                //continue throwing the exception
                throw;
            }

            Migrated.RaiseEvent(new MigrationEventArgs(migrations, migrationContext, _currentVersion, _targetVersion, false), this);

            return true;
        }

        /// <summary>
        /// Filters and orders migrations based on the migrations listed and the currently configured version and the target installation version
        /// </summary>
        /// <param name="foundMigrations"></param>
        /// <returns></returns>
        public IEnumerable<IMigration> OrderedUpgradeMigrations(IEnumerable<IMigration> foundMigrations)
        {
            var migrations = (from migration in foundMigrations
                              let migrationAttributes = migration.GetType().GetCustomAttributes<MigrationAttribute>(false)
                              from migrationAttribute in migrationAttributes
                              where migrationAttribute != null
                              where migrationAttribute.TargetVersion > _currentVersion &&
                                  migrationAttribute.TargetVersion <= _targetVersion &&
                                  migrationAttribute.ProductName == _productName &&
                    //filter if the migration specifies a minimum current version for which to execute
                    (migrationAttribute.MinimumCurrentVersion == null || _currentVersion >= migrationAttribute.MinimumCurrentVersion)
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
            var migrations = (from migration in foundMigrations
                              let migrationAttributes = migration.GetType().GetCustomAttributes<MigrationAttribute>(false)
                              from migrationAttribute in migrationAttributes
                              where migrationAttribute != null
                              where
                    migrationAttribute.TargetVersion > _currentVersion &&
                                  migrationAttribute.TargetVersion <= _targetVersion &&
                    migrationAttribute.ProductName == _productName &&
                    //filter if the migration specifies a minimum current version for which to execute
                    (migrationAttribute.MinimumCurrentVersion == null || _currentVersion >= migrationAttribute.MinimumCurrentVersion)
                              orderby migrationAttribute.TargetVersion, migrationAttribute.SortOrder descending
                              select migration).Distinct();
            return migrations;
        }

        /// <summary>
        /// Find all migrations that are available through the <see cref="MigrationResolver"/>
        /// </summary>
        /// <returns>An array of <see cref="IMigration"/></returns>
        protected virtual IMigration[] FindMigrations()
        {
            //MCH NOTE: Consider adding the ProductName filter to the Resolver so we don't get a bunch of irrelevant migrations
            return MigrationResolver.Current.Migrations.ToArray();
        }

        internal MigrationContext InitializeMigrations(List<IMigration> migrations, Database database, DatabaseProviders databaseProvider, bool isUpgrade = true)
        {
            //Loop through migrations to generate sql
            var context = new MigrationContext(databaseProvider, database, _logger);

            foreach (var migration in migrations)
            {
                var baseMigration = migration as MigrationBase;
                if (baseMigration != null)
                {
                    if (isUpgrade)
                    {
                        baseMigration.GetUpExpressions(context);
                        _logger.Info<MigrationRunner>(string.Format("Added UPGRADE migration '{0}' to context", baseMigration.GetType().Name));
                    }
                    else
                    {
                        baseMigration.GetDownExpressions(context);
                        _logger.Info<MigrationRunner>(string.Format("Added DOWNGRADE migration '{0}' to context", baseMigration.GetType().Name));
                    }
                }
                else
                {
                    //this is just a normal migration so we can only call Up/Down
                    if (isUpgrade)
                    {
                        migration.Up();
                        _logger.Info<MigrationRunner>(string.Format("Added UPGRADE migration '{0}' to context", migration.GetType().Name));
                    }
                    else
                    {
                        migration.Down();
                        _logger.Info<MigrationRunner>(string.Format("Added DOWNGRADE migration '{0}' to context", migration.GetType().Name));
                    }
                }
            }

            return context;
        }

        private void ExecuteMigrations(IMigrationContext context, Database database)
        {
            //Transactional execution of the sql that was generated from the found migrations
            using (var transaction = database.GetTransaction())
            {
                int i = 1;
                foreach (var expression in context.Expressions)
                {
                    var sql = expression.Process(database);
                    if (string.IsNullOrEmpty(sql))
                    {
                        i++;
                        continue;
                    }

                    _logger.Info<MigrationRunner>("Executing sql statement " + i + ": " + sql);
                    database.Execute(sql);
                    i++;
                }

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