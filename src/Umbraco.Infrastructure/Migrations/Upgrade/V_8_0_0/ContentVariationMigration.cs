
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class ContentVariationMigration : MigrationBase
{
    public ContentVariationMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {

    }

    // we *need* to use these private DTOs here, which does *not* have extra properties, which would kill the migration
}
