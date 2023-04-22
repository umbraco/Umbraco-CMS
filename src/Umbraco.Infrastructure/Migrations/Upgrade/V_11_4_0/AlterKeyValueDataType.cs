using System.Reflection;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_11_4_0;

public class AlterKeyValueDataType : MigrationBase
{
    public AlterKeyValueDataType(IMigrationContext context)
        : base(context)
    { }

    protected override void Migrate()
    {
        //SQL Lite doesn't need this upgrade
        if (Database.DatabaseType is not null &&
            Database.DatabaseType.GetProviderName()?.ToLower() == "system.data.sqlite")
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
