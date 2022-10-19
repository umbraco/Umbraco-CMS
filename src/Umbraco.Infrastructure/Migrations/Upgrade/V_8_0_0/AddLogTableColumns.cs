using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class AddLogTableColumns : MigrationBase
{
    public AddLogTableColumns(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        AddColumnIfNotExists<LogDto>(columns, "entityType");
        AddColumnIfNotExists<LogDto>(columns, "parameters");
    }
}
