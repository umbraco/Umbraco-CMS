﻿using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common
{
    public class CreateKeysAndIndexes : MigrationBase
    {
        public CreateKeysAndIndexes(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // remove those that may already have keys
            Delete.KeysAndIndexes(Cms.Core.Constants.DatabaseSchema.Tables.KeyValue).Do();
            Delete.KeysAndIndexes(Cms.Core.Constants.DatabaseSchema.Tables.PropertyData).Do();

            // re-create *all* keys and indexes
            foreach (var x in DatabaseSchemaCreator.OrderedTables)
                Create.KeysAndIndexes(x).Do();
        }
    }
}
