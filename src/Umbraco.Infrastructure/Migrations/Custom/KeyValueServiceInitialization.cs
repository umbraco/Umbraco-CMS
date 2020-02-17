using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Infrastructure.Migrations.Custom
{
    public class KeyValueServiceInitialization : IKeyValueServiceInitialization
    {
        private readonly ILogger _logger;

        public KeyValueServiceInitialization(ILogger logger)
        {
            _logger = logger;
        }

        public void PerformInitialMigration(IUmbracoDatabase database)
        {
            var context = new MigrationContext(database, _logger);
            var initMigration = new InitialMigration(context);
            initMigration.Migrate();
        }

        /// <summary>
        /// A custom migration that executes standalone during the Initialize phase of the KeyValueService.
        /// </summary>
        internal class InitialMigration : MigrationBase
        {
            public InitialMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                // as long as we are still running 7 this migration will be invoked,
                // but due to multiple restarts during upgrades, maybe the table
                // exists already
                if (TableExists(Constants.DatabaseSchema.Tables.KeyValue))
                    return;

                Logger.Info<KeyValueServiceInitialization>("Creating KeyValue structure.");

                // the locks table was initially created with an identity (auto-increment) primary key,
                // but we don't want this, especially as we are about to insert a new row into the table,
                // so here we drop that identity
                DropLockTableIdentity();

                // insert the lock object for key/value
                Insert.IntoTable(Constants.DatabaseSchema.Tables.Lock).Row(new { id = Constants.Locks.KeyValues, name = "KeyValues", value = 1 }).Do();

                // create the key-value table
                Create.Table<KeyValueDto>().Do();
            }

            private void DropLockTableIdentity()
            {
                // one cannot simply drop an identity, that requires a bit of work

                // create a temp. id column and copy values
                Alter.Table(Constants.DatabaseSchema.Tables.Lock).AddColumn("nid").AsInt32().Nullable().Do();
                Execute.Sql("update umbracoLock set nid = id").Do();

                // drop the id column entirely (cannot just drop identity)
                Delete.PrimaryKey("PK_umbracoLock").FromTable(Constants.DatabaseSchema.Tables.Lock).Do();
                Delete.Column("id").FromTable(Constants.DatabaseSchema.Tables.Lock).Do();

                // recreate the id column without identity and copy values
                Alter.Table(Constants.DatabaseSchema.Tables.Lock).AddColumn("id").AsInt32().Nullable().Do();
                Execute.Sql("update umbracoLock set id = nid").Do();

                // drop the temp. id column
                Delete.Column("nid").FromTable(Constants.DatabaseSchema.Tables.Lock).Do();

                // complete the primary key
                Alter.Table(Constants.DatabaseSchema.Tables.Lock).AlterColumn("id").AsInt32().NotNullable().PrimaryKey("PK_umbracoLock").Do();
            }
        }
    }
}
