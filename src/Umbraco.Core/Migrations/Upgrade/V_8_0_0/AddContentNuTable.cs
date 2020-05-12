using System.Data;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    class AddContentNuTable : MigrationBase
    {
        public AddContentNuTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            if (tables.InvariantContains("cmsContentNu")) return;

            Create.Table<ContentNuDto>(true).Do();
        }
    }
}
