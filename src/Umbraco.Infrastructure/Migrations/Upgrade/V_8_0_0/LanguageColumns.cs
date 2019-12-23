using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class LanguageColumns : MigrationBase
    {
        public LanguageColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            AddColumn<LanguageDto>(Constants.DatabaseSchema.Tables.Language, "isDefaultVariantLang");
            AddColumn<LanguageDto>(Constants.DatabaseSchema.Tables.Language, "mandatory");
        }
    }
}
