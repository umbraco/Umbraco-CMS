using System.Linq;
using NPoco;
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

            var existingTables = SqlSyntax.GetTablesInSchema(Context.Database).ToHashSet();

            // re-create *all* keys and indexes
            foreach (var entityClass in DatabaseSchemaCreator.OrderedTables)
            {
                var tableNameAttribute = entityClass.FirstAttribute<TableNameAttribute>();
                if (tableNameAttribute == null)
                    continue;

                if (!existingTables.Contains(tableNameAttribute.Value))
                    continue;

                Create.KeysAndIndexes(entityClass).Do();
            }
        }
    }
}
