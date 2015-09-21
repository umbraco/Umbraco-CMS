using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsContentTypeTable : MigrationBase
    {
        public UpdateCmsContentTypeTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Alter.Table("cmsContentType").AddColumn("isContainer").AsBoolean().NotNullable().WithDefaultValue(0);

            Alter.Table("cmsContentType").AddColumn("allowAtRoot").AsBoolean().NotNullable().WithDefaultValue(0);            
        }

        public override void Down()
        {
            Delete.Column("allowAtRoot").FromTable("cmsContentType");

            Delete.Column("isContainer").FromTable("cmsContentType");
        }
    }
}