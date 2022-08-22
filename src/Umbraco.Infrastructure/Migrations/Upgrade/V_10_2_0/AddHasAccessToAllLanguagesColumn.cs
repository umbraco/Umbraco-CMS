using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_2_0;

public class AddHasAccessToAllLanguagesColumn : MigrationBase
{
    public AddHasAccessToAllLanguagesColumn(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.UserGroup, "hasAccessToAllLanguages") is false)
        {
            Create.Column("hasAccessToAllLanguages")
                .OnTable(Constants.DatabaseSchema.Tables.UserGroup)
                .AsBoolean()
                .WithDefaultValue(true)
                .Do();
        }
    }
}
