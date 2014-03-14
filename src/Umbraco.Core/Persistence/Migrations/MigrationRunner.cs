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
        private readonly Version _configuredVersion;
        private readonly Version _targetVersion;
        private readonly string _productName;

        public MigrationRunner(Version configuredVersion, Version targetVersion, string productName)
        {
            _configuredVersion = configuredVersion;
            _targetVersion = targetVersion;
            _productName = productName;
        }

        /// <summary>
        /// Executes the migrations against the database.
        /// </summary>
        /// <param name="database">The PetaPoco Database, which the migrations will be run against</param>
        /// <param name="isUpgrade">Boolean indicating whether this is an upgrade or downgrade</param>
        /// <returns><c>True</c> if migrations were applied, otherwise <c>False</c></returns>
        public bool Execute(Database database, bool isUpgrade = true)
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
        public bool Execute(Database database, DatabaseProviders databaseProvider, bool isUpgrade = true)
        {
            LogHelper.Info<MigrationRunner>("Initializing database migrations");

	        var foundMigrations = MigrationResolver.Current.Migrations.ToArray();

            //filter all schema migrations
            var schemaMigrations = isUpgrade
                                 ? OrderedUpgradeMigrations(foundMigrations.Where(x => (x is SchemaMigration))).ToList()
                                 : OrderedDowngradeMigrations(foundMigrations.Where(x => (x is SchemaMigration))).ToList();

            //filter all non-schema migrations
            var dataMigrations = isUpgrade
                                 ? OrderedUpgradeMigrations(foundMigrations.Where(x => (x is SchemaMigration) == false)).ToList()
                                 : OrderedDowngradeMigrations(foundMigrations.Where(x => (x is SchemaMigration) == false)).ToList();
            
            //SD: Why do we want this?
            if (Migrating.IsRaisedEventCancelled(new MigrationEventArgs(dataMigrations, _configuredVersion, _targetVersion, true), this))
                return false;

            //Loop through migrations to generate sql
            var schemaMigrationContext = InitializeMigrations(schemaMigrations, database, databaseProvider, isUpgrade);
            
            try
            {
                ExecuteMigrations(schemaMigrationContext, database);
            }
            catch (Exception ex)
            {
                //if this fails then the transaction will be rolled back, BUT if we are using MySql this is not the case,
                //since it does not support schema changes in a transaction, see: http://dev.mysql.com/doc/refman/5.0/en/implicit-commit.html
                //so in that case we have to downgrade

                if (databaseProvider == DatabaseProviders.MySql)
                {
                    try
                    {
                        var downgrades = OrderedDowngradeMigrations(foundMigrations.Where(x => (x is SchemaMigration))).ToList();
                        var downgradeMigrationContext = InitializeMigrations(downgrades, database, databaseProvider, false);
                        //lets hope that works! - if something cannot be rolled back then a CatastrophicDataLossException should
                        // be thrown.
                        ExecuteMigrations(downgradeMigrationContext, database);
                    }
                    catch (Exception e)
                    {
                        throw new CatastrophicDataLossException(
                            "An error occurred running a schema migration but the changes could not be rolled back. Error: " + ex.Message + ". In some cases, it may be required that the database be restored to it's original state before running this upgrade process again.",
                            ex);

                    }
                }

                //continue throwing the exception
                throw;
            }

            //Ok, we've made it this far, now we can execute our data migrations

            //Loop through migrations to generate sql
            var dataMigrationContext = InitializeMigrations(dataMigrations, database, databaseProvider, isUpgrade);
            //run them - if this fails the data will be rolled back
            ExecuteMigrations(dataMigrationContext, database);

            Migrated.RaiseEvent(new MigrationEventArgs(dataMigrations, dataMigrationContext, _configuredVersion, _targetVersion, false), this);

            return true;
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

                    LogHelper.Info<MigrationRunner>("Executing sql statement " + i + ": " + sql);
                    database.Execute(sql);
                    i++;
                }

                transaction.Complete();
            }
	    }

        internal MigrationContext InitializeMigrations(List<IMigration> migrations, Database database, DatabaseProviders databaseProvider, bool isUpgrade = true)
	    {
            //Loop through migrations to generate sql
            var context = new MigrationContext(databaseProvider, database);
            
            foreach (var migration in migrations)
            {
                var baseMigration = migration as MigrationBase;
                if (baseMigration != null)
                {
                    if (isUpgrade)
                    {
                        baseMigration.GetUpExpressions(context);
                        LogHelper.Info<MigrationRunner>(string.Format("Added UPGRADE migration '{0}' to context", baseMigration.GetType().Name));
                    }
                    else
                    {
                        baseMigration.GetDownExpressions(context);
                        LogHelper.Info<MigrationRunner>(string.Format("Added DOWNGRADE migration '{0}' to context", baseMigration.GetType().Name));
                    }
                }
                else
                {
                    //this is just a normal migration so we can only call Up/Down
                    if (isUpgrade)
                    {
                        migration.Up();
                        LogHelper.Info<MigrationRunner>(string.Format("Added UPGRADE migration '{0}' to context", migration.GetType().Name));
                    }
                    else
                    {
                        migration.Down();
                        LogHelper.Info<MigrationRunner>(string.Format("Added DOWNGRADE migration '{0}' to context", migration.GetType().Name));
                    }
                }
            }

            return context;
	    }

        internal IEnumerable<IMigration> OrderedUpgradeMigrations(IEnumerable<IMigration> foundMigrations)
        {
            var migrations = (from migration in foundMigrations
                let migrationAttributes = migration.GetType().GetCustomAttributes<MigrationAttribute>(false)                   
                from migrationAttribute in migrationAttributes
                where migrationAttribute != null
                where
                    migrationAttribute.TargetVersion > _configuredVersion &&
                    migrationAttribute.TargetVersion <= _targetVersion &&
                    migrationAttribute.ProductName == _productName
                orderby migrationAttribute.TargetVersion, migrationAttribute.SortOrder ascending
                select migration).Distinct();
            return migrations;
        }

        public IEnumerable<IMigration> OrderedDowngradeMigrations(IEnumerable<IMigration> foundMigrations)
        {
            var migrations = (from migration in foundMigrations
                let migrationAttributes = migration.GetType().GetCustomAttributes<MigrationAttribute>(false)
                from migrationAttribute in migrationAttributes
                where migrationAttribute != null
                where
                    migrationAttribute.TargetVersion > _configuredVersion &&
                    migrationAttribute.TargetVersion <= _targetVersion &&
                    migrationAttribute.ProductName == _productName
                orderby migrationAttribute.TargetVersion, migrationAttribute.SortOrder descending
                select migration).Distinct();
            return migrations;
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