using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class MoveMasterContentTypeData : MigrationBase
    {
        public MoveMasterContentTypeData(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Reading entries from the cmsContentType table in order to update the parentID on the umbracoNode table.
            //NOTE This is primarily done because of a shortcoming in sql ce, which has really bad support for updates (can't use FROM or subqueries with multiple results).
            if (base.Context != null && base.Context.Database != null)
            {
                var list = base.Context.Database.Fetch<dynamic>("SELECT nodeId, masterContentType FROM cmsContentType WHERE not masterContentType is null AND masterContentType != 0");
                foreach (var item in list)
                {
                    Update.Table("umbracoNode").Set(new { parentID = item.masterContentType }).Where(new { id = item.nodeId });
                }
            }

            Execute.Sql(
                "INSERT INTO cmsContentType2ContentType (parentContentTypeId, childContentTypeId) SELECT masterContentType, nodeId FROM cmsContentType WHERE not masterContentType is null and masterContentType != 0");
        }

        public override void Down()
        {
        }
    }
}