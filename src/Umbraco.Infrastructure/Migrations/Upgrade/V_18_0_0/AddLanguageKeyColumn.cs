using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
/// Adds the <c>languageKey</c> column to the <c>umbracoLanguage</c> table
/// and populates it with a unique Guid for each existing row.
/// This must run BEFORE the EF Core snapshot migration so that EF Core
/// sees the column already present.
/// </summary>
public class AddLanguageKeyColumn : AsyncMigrationBase
{
    public AddLanguageKeyColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override async Task MigrateAsync()
    {
        var tableName = Constants.DatabaseSchema.Tables.Language;

        if (TableExists(tableName) is false)
        {
            return;
        }

        const string columnName = LanguageDto.LanguageKeyColumnName;

        if (ColumnExists(tableName, columnName))
        {
            return;
        }

        // Add the column as nullable first so existing rows don't fail
        AddColumn<LanguageDto>(tableName, columnName);

        // Populate each existing row with a new Guid
        var languages = await Database.FetchAsync<LanguageDto>($"SELECT * FROM {tableName}");
        foreach (LanguageDto language in languages)
        {
            language.LanguageKey = Guid.NewGuid();
            await Database.ExecuteAsync(
                $"UPDATE {tableName} SET {columnName} = @0 WHERE {LanguageDto.PrimaryKeyColumnName} = @1",
                [language.LanguageKey, language.Id]);
        }
    }
}
