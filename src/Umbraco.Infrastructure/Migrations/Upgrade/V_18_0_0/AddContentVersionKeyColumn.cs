using Umbraco.Cms.Core;
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
    const string indexName = "IX_umbracoContentVersion_key";

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

        // Add the column. Existing rows will get a default empty Guid.
        AddColumn<ContentVersionDto>(tableName, columnName);

        // Populate each existing row with a new Guid.
        var versions = await Database.FetchAsync<ContentVersionDto>($"SELECT * FROM {tableName}");
        foreach (ContentVersionDto version in versions)
        {
            version.Key = Guid.NewGuid();
            await Database.ExecuteAsync(
                $"UPDATE {tableName} SET {columnName} = @0 WHERE {ContentVersionDto.PrimaryKeyColumnName} = @1",
                [version.Key, version.Id]);
        }

        if (IndexExists(indexName) is false)
        {
            CreateIndex<ContentVersionDto>(indexName);
        }
    }
}
