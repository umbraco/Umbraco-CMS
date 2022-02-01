using Umbraco.Core.Migrations.Install;

namespace Umbraco.Core.Migrations.Upgrade.Common
{
    public class CreateKeysAndIndexes : MigrationBase
    {
        public CreateKeysAndIndexes(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // remove those that may already have keys
            Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.KeyValue).Do();
            Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.PropertyData).Do();

            // re-create *all* keys and indexes
            foreach (var x in DatabaseSchemaCreator.OrderedTables)
                Create.KeysAndIndexes(x).Do();
        }
    }
}
