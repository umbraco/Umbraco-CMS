using NPoco;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 6, GlobalSettings.UmbracoMigrationName)]
    public class RemoveMasterContentTypeColumn : MigrationBase
    {
        public RemoveMasterContentTypeColumn(ILogger logger) 
            : base(logger)
        { }

        public override void Up()
        {
            //NOTE Don't think we can remove this column yet as it seems to be used by some starterkits
            IfDatabase(DatabaseType.SQLCe, DatabaseType.SqlServer2005)
                .Delete.DefaultConstraint().OnTable("cmsContentType").OnColumn("masterContentType");

            Delete.Column("masterContentType").FromTable("cmsContentType");
        }

        public override void Down()
        {
            Create.UniqueConstraint("DF_cmsContentType_masterContentType").OnTable("cmsContentType").Column("masterContentType");

            Create.Column("masterContentType").OnTable("cmsContentType").AsInt16().Nullable().WithDefaultValue(0);
        }
    }
}