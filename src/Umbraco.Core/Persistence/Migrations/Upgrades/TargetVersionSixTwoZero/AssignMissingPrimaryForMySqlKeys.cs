using System.Linq;
using System.Web.UI;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    
    //see: http://issues.umbraco.org/issue/U4-4430
    [Migration("7.1.0", 0, GlobalSettings.UmbracoMigrationName)]
    [Migration("6.2.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AssignMissingPrimaryForMySqlKeys : MigrationBase
    {
        public AssignMissingPrimaryForMySqlKeys(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            if (Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();
                
                //This should be 2 because this table has 2 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsContentTypeAllowedContentType") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_cmsContentTypeAllowedContentType")
                        .OnTable("cmsContentTypeAllowedContentType")
                        .Columns(new[] { "Id", "AllowedId" });
                }

                //This should be 2 because this table has 2 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsDocumentType") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_cmsDocumentType")
                        .OnTable("cmsDocumentType")
                        .Columns(new[] { "contentTypeNodeId", "templateNodeId" });
                }

                //This should be 2 because this table has 2 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsMember2MemberGroup") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_cmsMember2MemberGroup")
                        .OnTable("cmsMember2MemberGroup")
                        .Columns(new[] { "Member", "MemberGroup" });
                }

                //This should be 2 because this table has 2 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsPreviewXml") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_cmsContentPreviewXml")
                        .OnTable("cmsPreviewXml")
                        .Columns(new[] { "nodeId", "versionId" });
                }

                //This should be 2 because this table has 2 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("umbracoUser2app") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_user2app")
                        .OnTable("umbracoUser2app")
                        .Columns(new[] { "user", "app" });
                }

                //This should be 2 because this table has 3 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("umbracoUser2NodeNotify") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_umbracoUser2NodeNotify")
                        .OnTable("umbracoUser2NodeNotify")
                        .Columns(new[] { "userId", "nodeId", "action" });
                }

                //This should be 2 because this table has 3 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("umbracoUser2NodePermission") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_umbracoUser2NodePermission")
                        .OnTable("umbracoUser2NodePermission")
                        .Columns(new[] { "userId", "nodeId", "permission" });
                }
            }
        }

        public override void Down()
        {
            //don't do anything, these keys should have always existed!
        }
    }
}