using Umbraco.Core.Migrations.PostMigrations;

namespace Umbraco.Core.Migrations.Upgrade.V_8_1_0
{
    public class ChangeNuCacheJsonFormat : MigrationBase
    {
        public ChangeNuCacheJsonFormat(IMigrationContext context) : base(context)
        { }

        public override void Migrate()
        {
            // nothing - just adding the post-migration
            Context.AddPostMigration<RebuildPublishedSnapshot>();
        }
    }
}
