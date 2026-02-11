using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;

/// <summary>
/// Increases the <c>type</c> column length in the <c>umbracoLongRunningOperation</c> table from 50 to 200
/// to accommodate longer operation type names (e.g. Examine index rebuild operations with custom index names).
/// </summary>
public class IncreaseSizeOfLongRunningOperationTypeColumn : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IncreaseSizeOfLongRunningOperationTypeColumn"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public IncreaseSizeOfLongRunningOperationTypeColumn(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        // SQLite doesn't need this - text is already unlimited
        if (DatabaseType == DatabaseType.SQLite)
        {
            return Task.CompletedTask;
        }

        const string ColumnName = "type";
        const int ColumnLength = 200;

        // Check if column is already 200 or larger
        // Skip migration if already migrated to prevent re-running
        var maxLength = Database.ExecuteScalar<int?>(
            @"SELECT CHARACTER_MAXIMUM_LENGTH
              FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = @0
              AND COLUMN_NAME = @1
              AND TABLE_SCHEMA = SCHEMA_NAME()",
            Constants.DatabaseSchema.Tables.LongRunningOperation,
            ColumnName);

        if (maxLength >= ColumnLength)
        {
            return Task.CompletedTask;
        }

        Alter.Table(Constants.DatabaseSchema.Tables.LongRunningOperation)
            .AlterColumn(ColumnName)
            .AsString(ColumnLength)
            .NotNullable()
            .Do();

        return Task.CompletedTask;
    }
}
