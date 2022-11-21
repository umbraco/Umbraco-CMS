using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_6_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
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
