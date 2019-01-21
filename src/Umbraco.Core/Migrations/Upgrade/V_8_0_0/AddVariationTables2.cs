using System;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddVariationTables2 : MigrationBase
    {
        public AddVariationTables2(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Create.Table<ContentVersionCultureVariationDto>().Do();
            Create.Table<DocumentCultureVariationDto>().Do();
        }
    }
}
