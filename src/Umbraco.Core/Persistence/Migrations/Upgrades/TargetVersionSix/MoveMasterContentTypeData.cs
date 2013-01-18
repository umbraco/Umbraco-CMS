using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class MoveMasterContentTypeData : MigrationBase
    {
        public override void Up()
        {
            Execute.Sql(
                "INSERT INTO cmsContentType2ContentType (parentContentTypeId, childContentTypeId) SELECT masterContentType, nodeId FROM cmsContentType WHERE not masterContentType is null and masterContentType != 0");

            Execute.Sql("UPDATE umbracoNode SET parentID = cmsContentType.masterContentType FROM umbracoNode, cmsContentType WHERE umbracoNode.id = cmsContentType.nodeId AND not cmsContentType.masterContentType is null AND cmsContentType.masterContentType != 0");
        }

        public override void Down()
        {
        }
    }
}