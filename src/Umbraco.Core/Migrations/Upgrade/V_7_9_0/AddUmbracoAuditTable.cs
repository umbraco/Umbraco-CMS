using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_9_0
{
    internal class AddUmbracoAuditTable : MigrationBase
    {
        public AddUmbracoAuditTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains(Constants.DatabaseSchema.Tables.AuditEntry))
                return;

            Create.Table<AuditEntryDto>().Do();
        }
    }
}
