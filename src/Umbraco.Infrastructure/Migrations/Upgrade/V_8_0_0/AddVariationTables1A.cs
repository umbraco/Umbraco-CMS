using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class AddVariationTables1A : MigrationBase
{
    public AddVariationTables1A(IMigrationContext context)
        : base(context)
    {
    }

    // note - original AddVariationTables1 just did
    // Create.Table<ContentVersionCultureVariationDto>().Do();
    //
    // this is taking care of ppl left in this state
    protected override void Migrate()
    {
        // note - original AddVariationTables1 just did
        // Create.Table<ContentVersionCultureVariationDto>().Do();
        //
        // it's been deprecated, not part of the main upgrade path,
        // but we need to take care of ppl caught into the state

        // was not used
        Delete.Column("available").FromTable(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation).Do();

        // was not used
        Delete.Column("availableDate").FromTable(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation).Do();

        // special trick to add the column without constraints and return the sql to add them later
        AddColumn<ContentVersionCultureVariationDto>("date", out IEnumerable<string> sqls);

        // now we need to update the new column with some values because this column doesn't allow NULL values
        Update.Table(ContentVersionCultureVariationDto.TableName).Set(new { date = DateTime.Now }).AllRows().Do();

        // now apply constraints (NOT NULL) to new table
        foreach (var sql in sqls)
        {
            Execute.Sql(sql).Do();
        }

        // name, languageId are now non-nullable
        AlterColumn<ContentVersionCultureVariationDto>(
            Constants.DatabaseSchema.Tables.ContentVersionCultureVariation,
            "name");
        AlterColumn<ContentVersionCultureVariationDto>(
            Constants.DatabaseSchema.Tables.ContentVersionCultureVariation,
            "languageId");

        Create.Table<DocumentCultureVariationDto>().Do();
    }
}
