using Umbraco.Core.Migrations.Install;

namespace Umbraco.Core.Migrations.Upgrade.Post
{
    public class CreateKeysAndIndexes : MigrationBase
    {
        public CreateKeysAndIndexes(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // really make sure there is nothing left
            Delete.KeysAndIndexes().Do();

            // re-create *all* keys and indexes
            foreach (var x in DatabaseSchemaCreator.OrderedTables)
                Create.KeysAndIndexes(x).Do();
        }
    }
}
