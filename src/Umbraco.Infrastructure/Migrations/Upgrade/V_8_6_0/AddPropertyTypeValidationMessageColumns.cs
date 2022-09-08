using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_6_0;

public class AddPropertyTypeValidationMessageColumns : MigrationBase
{
    public AddPropertyTypeValidationMessageColumns(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        AddColumnIfNotExists<PropertyTypeDto>(columns, "mandatoryMessage");
        AddColumnIfNotExists<PropertyTypeDto>(columns, "validationRegExpMessage");
    }
}
