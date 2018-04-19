using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddContentVariationTable : MigrationBase
    {
        public AddContentVariationTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Create.Table<ContentVersionCultureVariationDto>();

            // fixme - data migration?
        }
    }
}
