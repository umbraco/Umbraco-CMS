using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class RenameCmsTabTable : MigrationBase
    {
        public override void Up()
        {
            Rename.Table("cmsTab").To("cmsPropertyTypeGroup");
        }

        public override void Down()
        {
            Rename.Table("cmsPropertyTypeGroup").To("cmsTab");
        }
    }
}