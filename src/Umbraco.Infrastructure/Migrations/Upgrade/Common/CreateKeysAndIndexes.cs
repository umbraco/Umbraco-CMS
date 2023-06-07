using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common;

public class CreateKeysAndIndexes : MigrationBase
{
    public CreateKeysAndIndexes(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // remove those that may already have keys
        Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.KeyValue).Do();
        Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.PropertyData).Do();

        // re-create *all* keys and indexes
        foreach (Type x in DatabaseSchemaCreator._orderedTables)
        {
            Create.KeysAndIndexes(x).Do();
        }
    }
}
