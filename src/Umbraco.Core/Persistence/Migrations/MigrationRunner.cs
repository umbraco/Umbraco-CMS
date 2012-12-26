using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;

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

        public MigrationRunner(Version configuredVersion, Version targetVersion)
        {
            _configuredVersion = configuredVersion;
            _targetVersion = targetVersion;
        }

        /// <summary>
        /// Executes the migrations against the database.
        /// </summary>
        /// <param name="database">The PetaPoco Database, which the migrations will be run against</param>
        /// <param name="isUpgrade">Boolean indicating whether this is an upgrade or downgrade</param>
        /// <returns>True if migrations were applied, otherwise False</returns>
        public bool Execute(Database database, bool isUpgrade)
        {
            LogHelper.Info<MigrationRunner>("Initializing database migration");

            var foundMigrations = PluginManager.Current.FindMigrations();
            var migrations = isUpgrade
                                 ? OrderedUpgradeMigrations(foundMigrations)
                                 : OrderedDowngradeMigrations(foundMigrations);

            //Loop through migrations to generate sql
            var context = new MigrationContext();
            foreach (MigrationBase migration in migrations)
            {
                if (isUpgrade)
                {
                    migration.GetUpExpressions(context);
                }
                else
                {
                    migration.GetDownExpressions(context);
                }
            }

            //Transactional execution of the sql that was generated from the found migrations
            using (Transaction transaction = database.GetTransaction())
            {
                foreach (var expression in context.Expressions)
                {
                    var sql = expression.ToString();
                    LogHelper.Info<MigrationRunner>("Executing sql: " + sql);
                    database.Execute(sql);
                }

                transaction.Complete();
            }

            return true;
        }

        internal IEnumerable<IMigration> OrderedUpgradeMigrations(IEnumerable<IMigration> foundMigrations)
        {
            var migrations = (from migration in foundMigrations
                              let migrationAttribute = migration.GetType().FirstAttribute<MigrationAttribute>()
                              where migrationAttribute != null
                              where
                                  migrationAttribute.TargetVersion > _configuredVersion &&
                                  migrationAttribute.TargetVersion <= _targetVersion
                              orderby migrationAttribute.SortOrder ascending 
                              select migration);
            return migrations;
        }

        public IEnumerable<IMigration> OrderedDowngradeMigrations(IEnumerable<IMigration> foundMigrations)
        {
            var migrations = (from migration in foundMigrations
                              let migrationAttribute = migration.GetType().FirstAttribute<MigrationAttribute>()
                              where migrationAttribute != null
                              where
                                  migrationAttribute.TargetVersion > _configuredVersion &&
                                  migrationAttribute.TargetVersion <= _targetVersion
                              orderby migrationAttribute.SortOrder descending 
                              select migration);
            return migrations;
        }
    }
}