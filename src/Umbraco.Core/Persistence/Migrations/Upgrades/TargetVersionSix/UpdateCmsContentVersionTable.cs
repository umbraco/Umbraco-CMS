using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 8, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsContentVersionTable : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("cmsContentVersion").AddColumn("LanguageLocale").AsString(10).Nullable();
        }

        public override void Down()
        {
            Delete.Column("LanguageLocale").FromTable("cmsContentVersion");
        }
    }
}