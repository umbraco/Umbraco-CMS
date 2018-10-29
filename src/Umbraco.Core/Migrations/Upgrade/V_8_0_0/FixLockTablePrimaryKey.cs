using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class FixLockTablePrimaryKey : MigrationBase
    {
        public FixLockTablePrimaryKey(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // at some point, the KeyValueService dropped the PK and failed to re-create it,
            // so the PK is gone - make sure we have one, and create if needed

            var constraints = SqlSyntax.GetConstraintsPerTable(Database);
            var exists = constraints.Any(x => x.Item2 == "PK_umbracoLock");

            if (!exists)
                Create.PrimaryKey("PK_umbracoLock").OnTable(Constants.DatabaseSchema.Tables.Lock).Column("id").Do();
        }
    }
}
