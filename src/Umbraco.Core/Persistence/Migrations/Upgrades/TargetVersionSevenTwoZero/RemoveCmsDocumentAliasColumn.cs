using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwoZero
{
    [Migration("7.2.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class RemoveCmsDocumentAliasColumn : MigrationBase
    {
        public override void Up()
        {
            Delete.Column("alias").FromTable("cmsDocument");
        }

        public override void Down()
        {
        }
    }
}