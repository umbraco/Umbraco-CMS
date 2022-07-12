using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class LanguageColumns : MigrationBase
{
    public LanguageColumns(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        AddColumn<LanguageDto>(Constants.DatabaseSchema.Tables.Language, "isDefaultVariantLang");
        AddColumn<LanguageDto>(Constants.DatabaseSchema.Tables.Language, "mandatory");
    }
}
