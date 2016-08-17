using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 8, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsContentVersionTable : MigrationBase
    {
        public UpdateCmsContentVersionTable(IMigrationContext context) 
            : base(context)
        { }


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