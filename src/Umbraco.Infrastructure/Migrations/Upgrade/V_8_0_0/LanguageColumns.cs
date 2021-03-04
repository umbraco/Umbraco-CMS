﻿using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0
{
    public class LanguageColumns : MigrationBase
    {
        public LanguageColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            AddColumn<LanguageDto>(Cms.Core.Constants.DatabaseSchema.Tables.Language, "isDefaultVariantLang");
            AddColumn<LanguageDto>(Cms.Core.Constants.DatabaseSchema.Tables.Language, "mandatory");
        }
    }
}
