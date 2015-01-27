using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class UpdateUniqueIdToHaveCorrectIndexType : MigrationBase
    {
        //see: http://issues.umbraco.org/issue/U4-6188, http://issues.umbraco.org/issue/U4-6187
        public override void Up()
        {
            //must be non-nullable
            Alter.Column("uniqueID").OnTable("umbracoNode").AsGuid().NotNullable();
            //must be a uniqe index
            Delete.Index("IX_umbracoNodeUniqueID").OnTable("umbracoNode");
            Create.Index("IX_umbracoNode_uniqueID").OnTable("umbracoNode").OnColumn("uniqueID").Unique();
        }

        public override void Down()
        {
        }
    }
}