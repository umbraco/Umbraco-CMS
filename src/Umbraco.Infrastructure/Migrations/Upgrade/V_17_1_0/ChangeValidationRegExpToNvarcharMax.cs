using Umbraco.Cms.Core;
using NPoco;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_1_0;

/// <summary>
/// Represents a migration that alters the <c>ValidationRegExp</c> column to use <c>nvarchar(max)</c>, allowing for longer regular expression patterns to be stored.
/// </summary>
public class ChangeValidationRegExpToNvarcharMax : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_1_0.ChangeValidationRegExpToNvarcharMax"/> class for updating the validation regular expression column type.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration.</param>
    public ChangeValidationRegExpToNvarcharMax(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // SQLite doesn't need this - text is already unlimited
        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        // Check if column is already nvarchar(max) (CHARACTER_MAXIMUM_LENGTH = -1)
        // Skip migration if already migrated to prevent re-running
        var maxLength = Database.ExecuteScalar<int?>(
            @"SELECT CHARACTER_MAXIMUM_LENGTH
              FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = @0
              AND COLUMN_NAME = @1
              AND TABLE_SCHEMA = SCHEMA_NAME()",
            Constants.DatabaseSchema.Tables.PropertyType,
            "validationRegExp");

        // -1 means nvarchar(max) - already migrated
        if (maxLength == -1)
        {
            return;
        }

        Alter.Table(Constants.DatabaseSchema.Tables.PropertyType)
            .AlterColumn("validationRegExp")
            .AsCustom("nvarchar(max)")
            .Nullable()
            .Do();
    }
}
