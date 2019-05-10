using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddLockTable : MigrationBase
    {
        public AddLockTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            if (tables.InvariantContains("umbracoLock"))
                return;

            Create.Table<LockDto>(true);
        }
    }
}
