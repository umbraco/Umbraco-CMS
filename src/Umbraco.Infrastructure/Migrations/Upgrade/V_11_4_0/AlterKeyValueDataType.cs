using System.Reflection;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_11_4_0;

[Obsolete("This is no longer used and will be removed in V14.")]
public class AlterKeyValueDataType : MigrationBase
{
    private readonly IMigrationContext _context;

    public AlterKeyValueDataType(IMigrationContext context)
        : base(context) => _context = context;

    protected override void Migrate()
    {
        // SQLite doesn't need this upgrade
        if (_context.Database.DatabaseType.IsSqlite())
        {
            return;
        }

        string tableName = KeyValueDto.TableName;
        string colName = "value";

        // Determine the current datatype of the column within the database
        string colDataType = Database.ExecuteScalar<string>($"SELECT TOP(1) CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS" +
            $" WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{colName}'");

        // 255 is the old length, -1 indicate MAX length
        if (colDataType == "255")
        {
            // Upgrade to MAX length
            Database.Execute($"ALTER TABLE {tableName} ALTER COLUMN {colName} nvarchar(MAX)");
        }
    }
}
