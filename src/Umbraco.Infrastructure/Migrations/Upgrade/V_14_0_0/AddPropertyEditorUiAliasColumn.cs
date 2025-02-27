using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class AddPropertyEditorUiAliasColumn : MigrationBase
{
    public AddPropertyEditorUiAliasColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.DataType, "propertyEditorUiAlias") is false)
        {
            Create.Column("propertyEditorUiAlias")
                .OnTable(Constants.DatabaseSchema.Tables.DataType)
                .AsString(255)
                .Nullable()
                .Do();
        }
    }
}
