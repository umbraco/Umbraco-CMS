using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Adds the <c>key</c> column to the <c>umbracoContentVersion</c> table
/// and populates it with a unique Guid for each existing row.
/// This must run BEFORE the EF Core snapshot migration so that EF Core
/// sees the column already present.
/// </summary>
public class AddContentVersionKeyColumn : AsyncMigrationBase
{
    private const string IndexName = "IX_umbracoContentVersion_key";

    public AddContentVersionKeyColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override async Task MigrateAsync()
    {
        var tableName = Constants.DatabaseSchema.Tables.ContentVersion;

        if (TableExists(tableName) is false)
        {
            return;
        }

        const string columnName = ContentVersionDto.KeyColumnName;

        if (ColumnExists(tableName, columnName))
        {
            return;
        }

        AddColumn<ContentVersionDto>(tableName, columnName);

        await AssignKeysToExistingRowsAsync(tableName, columnName);

        if (IndexExists(IndexName) is false)
        {
            CreateIndex<ContentVersionDto>(IndexName);
        }
    }

    /// <summary>
    /// Gives every row a distinct key. SQL Server evaluates its <c>NEWID()</c> default per row while adding the
    /// column, so only SQLite - whose default is a single placeholder value - has rows left to fill.
    /// </summary>
    private async Task AssignKeysToExistingRowsAsync(string tableName, string columnName)
    {
        if (DatabaseType.IsSqlite() is false)
        {
            return;
        }

        var quotedTable = SqlSyntax.GetQuotedTableName(tableName);
        var quotedColumn = SqlSyntax.GetQuotedColumnName(columnName);

        // A version 4 Guid built from SQLite's own randomness, uppercase to match how Guids are written here.
        // random() is re-evaluated per row, so each row gets its own value.
        await Database.ExecuteAsync(
            $"""
             UPDATE {quotedTable}
             SET {quotedColumn} = upper(
                 substr(hex(randomblob(4)), 1, 8) || '-' ||
                 substr(hex(randomblob(2)), 1, 4) || '-4' ||
                 substr(hex(randomblob(2)), 2, 3) || '-' ||
                 substr('89ab', 1 + (abs(random()) % 4), 1) ||
                 substr(hex(randomblob(2)), 2, 3) || '-' ||
                 substr(hex(randomblob(6)), 1, 12))
             WHERE {quotedColumn} = @0
             """,
            new object[] { Guid.Empty.ToString() });
    }
}
