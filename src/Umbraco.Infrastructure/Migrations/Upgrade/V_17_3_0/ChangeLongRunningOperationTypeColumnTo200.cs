using NPoco;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;

/// <summary>
/// Increases the <c>type</c> column length in the <c>umbracoLongRunningOperation</c> table from 50 to 200
/// to accommodate longer operation type names (e.g. Examine index rebuild operations with custom index names).
/// </summary>
public class ChangeLongRunningOperationTypeColumnTo200 : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeLongRunningOperationTypeColumnTo200"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public ChangeLongRunningOperationTypeColumnTo200(IMigrationContext context)
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

        // Check if column is already 200 or larger
        // Skip migration if already migrated to prevent re-running
        var maxLength = Database.ExecuteScalar<int?>(
            @"SELECT CHARACTER_MAXIMUM_LENGTH
              FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = @0
              AND COLUMN_NAME = @1
              AND TABLE_SCHEMA = SCHEMA_NAME()",
            Constants.DatabaseSchema.Tables.LongRunningOperation,
            "type");

        if (maxLength >= 200)
        {
            return Task.CompletedTask;
        }

        Alter.Table(Constants.DatabaseSchema.Tables.LongRunningOperation)
            .AlterColumn("type")
            .AsString(200)
            .NotNullable()
            .Do();

        return Task.CompletedTask;
    }
}
