using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;

/// <summary>
/// Migration to add the editableInVisualEditor column to the cmsPropertyType table.
/// </summary>
public class AddEditableInVisualEditorToPropertyType : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddEditableInVisualEditorToPropertyType"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddEditableInVisualEditorToPropertyType(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.PropertyType) is false)
        {
            return;
        }

        const string columnName = "editableInVisualEditor";
        var hasColumn = Context.SqlContext.SqlSyntax.GetColumnsInSchema(Context.Database)
            .Any(c =>
                c.TableName == Constants.DatabaseSchema.Tables.PropertyType &&
                c.ColumnName == columnName);

        if (hasColumn)
        {
            return;
        }

        AddColumn<PropertyTypeDto>(Constants.DatabaseSchema.Tables.PropertyType, columnName);
    }
}
