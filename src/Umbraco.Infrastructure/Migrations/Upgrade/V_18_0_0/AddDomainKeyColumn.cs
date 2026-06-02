using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Adds the <c>key</c> column to the <c>umbracoDomain</c> table
/// and populates it with a unique Guid for each existing row.
/// This must run BEFORE the EF Core snapshot migration so that EF Core
/// sees the column already present.
/// </summary>
public class AddDomainKeyColumn : AsyncMigrationBase
{
    const string indexName = "IX_umbracoDomain_key";

    public AddDomainKeyColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override async Task MigrateAsync()
    {
        var tableName = Constants.DatabaseSchema.Tables.Domain;

        if (TableExists(tableName) is false)
        {
            return;
        }

        const string columnName = DomainDto.KeyColumnName;

        if (ColumnExists(tableName, columnName))
        {
            return;
        }

        // Add the column. existing rows will get a default empty Guid
        AddColumn<DomainDto>(tableName, columnName);

        // Populate each existing row with a new Guid
        var domains = await Database.FetchAsync<DomainDto>($"SELECT * FROM {tableName}");
        foreach (DomainDto domain in domains)
        {
            domain.Key = Guid.NewGuid();
            await Database.ExecuteAsync(
                $"UPDATE {tableName} SET {columnName} = @0 WHERE {DomainDto.PrimaryKeyColumnName} = @1",
                [domain.Key, domain.Id]);
        }

        if (IndexExists(indexName) is false)
        {
            CreateIndex<DomainDto>(indexName);
        }
    }
}
