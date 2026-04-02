using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Migration that adds the <c>UiAlias</c> column to the <c>PropertyEditor</c> table during the upgrade to version 14.0.0.
/// </summary>
public class AddPropertyEditorUiAliasColumn : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPropertyEditorUiAliasColumn"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> providing migration information and services.</param>
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
