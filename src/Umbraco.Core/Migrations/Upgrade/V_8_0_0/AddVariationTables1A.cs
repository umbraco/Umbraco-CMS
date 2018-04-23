using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddVariationTables1A : MigrationBase
    {
        public AddVariationTables1A(IMigrationContext context)
            : base(context)
        { }

        // note - original AddVariationTables1 just did
        // Create.Table<ContentVersionCultureVariationDto>().Do();
        //
        // this is taking care of ppl left in this state

        public override void Migrate()
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
            AddColumn<ContentVersionCultureVariationDto>("date");

            // name, languageId are now non-nullable
            AlterColumn<ContentVersionCultureVariationDto>(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation, "name");
            AlterColumn<ContentVersionCultureVariationDto>(Constants.DatabaseSchema.Tables.ContentVersionCultureVariation, "languageId");           

            Create.Table<DocumentCultureVariationDto>().Do();

            // fixme - data migration?
        }
    }
}
