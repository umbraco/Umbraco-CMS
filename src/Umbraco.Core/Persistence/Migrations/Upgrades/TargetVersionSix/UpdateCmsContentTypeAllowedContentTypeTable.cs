using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 3, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsContentTypeAllowedContentTypeTable : MigrationBase
    {
        public UpdateCmsContentTypeAllowedContentTypeTable(IMigrationContext context) 
            : base(context)
        { }


        public override void Up()
        {
            Alter.Table("cmsContentTypeAllowedContentType").AddColumn("sortOrder").AsInt16().NotNullable().WithDefaultValue(1);
        }

        public override void Down()
        {
            Delete.Column("sortOrder").FromTable("cmsContentTypeAllowedContentType");
        }
    }
}