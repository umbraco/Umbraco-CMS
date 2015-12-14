using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// Represents the Migration Runner, which is used to apply migrations to
    /// the umbraco database.
    /// </summary>
    public class MigrationRunner
    {
        private readonly IMigrationEntryService _migrationEntryService;
        private readonly ILogger _logger;
        private readonly SemVersion _currentVersion;
        private readonly SemVersion _targetVersion;
        private readonly string _productName;
        private readonly IMigration[] _migrations;

        [Obsolete("Use the ctor that specifies all dependencies instead")]
        public MigrationRunner(Version currentVersion, Version targetVersion, string productName)
            : this(LoggerResolver.Current.Logger, currentVersion, targetVersion, productName)
        {
        }

        [Obsolete("Use the ctor that specifies all dependencies instead")]
        public MigrationRunner(ILogger logger, Version currentVersion, Version targetVersion, string productName)
            : this(logger, currentVersion, targetVersion, productName, null)
        {
        }

        [Obsolete("Use the ctor that specifies all dependencies instead")]
        public MigrationRunner(ILogger logger, Version currentVersion, Version targetVersion, string productName, params IMigration[] migrations)
            : this(ApplicationContext.Current.Services.MigrationEntryService, logger, new SemVersion(currentVersion), new SemVersion(targetVersion), productName, migrations)
        {
            
        }

        public MigrationRunner(IMigrationEntryService migrationEntryService, ILogger logger, SemVersion currentVersion, SemVersion targetVersion, string productName, params IMigration[] migrations)
        {
            if (migrationEntryService == null) throw new ArgumentNullException("migrationEntryService");
            if (logger == null) throw new ArgumentNullException("logger");
            if (currentVersion == null) throw new ArgumentNullException("currentVersion");
            if (targetVersion == null) throw new ArgumentNullException("targetVersion");
            Mandate.ParameterNotNullOrEmpty(productName, "productName");

            _migrationEntryService = migrationEntryService;
            _logger = logger;
            _currentVersion = currentVersion;
            _targetVersion = targetVersion;
            _productName = productName;
            //ensure this is null if there aren't any
            _migrations = migrations.Length == 0 ? null : migrations;
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

            
            if (Migrating.IsRaisedEventCancelled(new MigrationEventArgs(migrations, _currentVersion, _targetVersion, true), this))
            {
                _logger.Warn<MigrationRunner>("Migration was cancelled by an event");
                return false;
            }

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
        protected virtual IMigration[] FindMigrations()
        {
            //MCH NOTE: Consider adding the ProductName filter to the Resolver so we don't get a bunch of irrelevant migrations
            return _migrations ?? MigrationResolver.Current.Migrations.ToArray();
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

                    //TODO: We should output all of these SQL calls to files in a migration folder in App_Data/TEMP
                    // so if people want to executed them manually on another environment, they can.

                    //The following ensures the multiple statement sare executed one at a time, this is a requirement
                    // of SQLCE, it's unfortunate but necessary.
                    // http://stackoverflow.com/questions/13665491/sql-ce-inconsistent-with-multiple-statements
                    var sb = new StringBuilder();
                    using (var reader = new StringReader(sql))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.Equals("GO", StringComparison.OrdinalIgnoreCase))
                            {
                                //Execute the SQL up to the point of a GO statement
                                var exeSql = sb.ToString();
                                _logger.Info<MigrationRunner>("Executing sql statement " + i + ": " + exeSql);
                                database.Execute(exeSql);
                                
                                //restart the string builder
                                sb.Remove(0, sb.Length);
                            }
                            else
                            {
                                sb.AppendLine(line);
                            }
                        }
                        //execute anything remaining
                        if (sb.Length > 0)
                        {
                            var exeSql = sb.ToString();
                            _logger.Info<MigrationRunner>("Executing sql statement " + i + ": " + exeSql);
                            database.Execute(exeSql);
                        }
                    }
                    
                    i++;
                }

                transaction.Complete();

                //Now that this is all complete, we need to add an entry to the migrations table flagging that migrations
                // for this version have executed.
                //NOTE: We CANNOT do this as part of the transaction!!! This is because when upgrading to 7.3, we cannot
                // create the migrations table and then add data to it in the same transaction without issuing things like GO
                // commands and since we need to support all Dbs, we need to just do this after the fact.
                var exists = _migrationEntryService.FindEntry(GlobalSettings.UmbracoMigrationName, _targetVersion);
                if (exists == null)
                {
                    _migrationEntryService.CreateEntry(_productName, _targetVersion);    
                }
               
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