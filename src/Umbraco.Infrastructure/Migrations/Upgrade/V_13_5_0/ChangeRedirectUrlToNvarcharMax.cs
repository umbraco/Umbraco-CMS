using System.Linq.Expressions;
using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_5_0;

public class ChangeRedirectUrlToNvarcharMax : MigrationBase
{
    public ChangeRedirectUrlToNvarcharMax(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        // We don't need to run this migration for SQLite, since ntext is not a thing there, text is just text.
        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        string tableName = RedirectUrlDto.TableName;
        string colName = "url";

        // Determine the current datatype of the column within the database
        string colDataType = Database.ExecuteScalar<string>($"SELECT TOP(1) CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS" +
                                                            $" WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{colName}'");

        // 255 is the old length, -1 indicate MAX length
        if (colDataType == "255")
        {
            // Upgrade to MAX length
            Database.Execute($"Drop Index IX_umbracoRedirectUrl_culture_hash on {Constants.DatabaseSchema.Tables.RedirectUrl}");
            Database.Execute($"ALTER TABLE {tableName} ALTER COLUMN {colName} nvarchar(MAX) NOT NULL");
            Database.Execute($"CREATE INDEX IX_umbracoRedirectUrl_culture_hash ON {Constants.DatabaseSchema.Tables.RedirectUrl} (urlHash, contentKey, culture, createDateUtc)");
        }
    }
}
