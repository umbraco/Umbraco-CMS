using System;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("6.2.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AdditionalIndexesAndKeys : MigrationBase
    {
        public override void Up()
        {          
            Create.Index("IX_umbracoNodeTrashed").OnTable("umbracoNode").OnColumn("trashed").Ascending().WithOptions().NonClustered();
            Create.Index("IX_cmsContentVersion_ContentId").OnTable("cmsContentVersion").OnColumn("ContentId").Ascending().WithOptions().NonClustered();
            Create.Index("IX_cmsDocument_published").OnTable("cmsDocument").OnColumn("published").Ascending().WithOptions().NonClustered();
            Create.Index("IX_cmsDocument_newest").OnTable("cmsDocument").OnColumn("newest").Ascending().WithOptions().NonClustered();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }

    [Migration("6.2.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class ChangePasswordColumn : MigrationBase
    {
        public override void Up()
        {
            //up to 500 chars
            Alter.Table("umbracoUser").AlterColumn("userPassword").AsString(500).NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}