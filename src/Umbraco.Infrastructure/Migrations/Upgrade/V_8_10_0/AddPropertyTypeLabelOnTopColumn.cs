using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_10_0;

public class AddPropertyTypeLabelOnTopColumn : MigrationBase
{
    public AddPropertyTypeLabelOnTopColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        AddColumnIfNotExists<PropertyTypeDto>(columns, "labelOnTop");
    }
}
