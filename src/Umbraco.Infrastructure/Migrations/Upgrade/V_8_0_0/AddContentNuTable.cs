using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

internal class AddContentNuTable : MigrationBase
{
    public AddContentNuTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
        if (tables.InvariantContains("cmsContentNu"))
        {
            return;
        }

        Create.Table<ContentNuDto>(true).Do();
    }
}
