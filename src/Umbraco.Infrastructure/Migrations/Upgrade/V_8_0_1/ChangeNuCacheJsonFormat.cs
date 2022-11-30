using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_1;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class ChangeNuCacheJsonFormat : MigrationBase
{
    public ChangeNuCacheJsonFormat(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() =>

        // nothing - just adding the post-migration
        Context.AddPostMigration<RebuildPublishedSnapshot>();
}
