using System.Data;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwoZero
{
    [Migration("7.2.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class AddMissingForeignKeyForContentType : MigrationBase
    {
        public override void Up()
        {
            //SELECT * FROM cmsContent WHERE NOT EXISTS (SELECT cmsContentType.nodeId FROM cmsContentType WHERE cmsContentType.nodeId = cmsContent.contentType)

            //Before we can add the foreign key there cannot be any orphaned content, media, members
            var orphanedIds = Context.Database.Fetch<int>("SELECT contentType FROM cmsContent WHERE NOT EXISTS (SELECT cmsContentType.nodeId FROM cmsContentType WHERE cmsContentType.nodeId = cmsContent.contentType)");
            foreach (var orphanedId in orphanedIds)
            {
                Delete.FromTable("cmsTask").Row(new { nodeId = orphanedId });
                Delete.FromTable("umbracoUser2NodeNotify").Row(new { nodeId = orphanedId });
                Delete.FromTable("umbracoUser2NodePermission").Row(new { nodeId = orphanedId });
                Delete.FromTable("umbracoRelation").Row(new { parentId = orphanedId });
                Delete.FromTable("umbracoRelation").Row(new { childId = orphanedId });
                Delete.FromTable("cmsTagRelationship").Row(new { nodeId = orphanedId });
                Delete.FromTable("umbracoDomains").Row(new { domainRootStructureID = orphanedId });
                Delete.FromTable("cmsDocument").Row(new { nodeId = orphanedId });
                Delete.FromTable("cmsPropertyData").Row(new { contentNodeId = orphanedId });
                Delete.FromTable("cmsMember2MemberGroup").Row(new { Member = orphanedId });
                Delete.FromTable("cmsMember").Row(new { nodeId = orphanedId });
                Delete.FromTable("cmsPreviewXml").Row(new { nodeId = orphanedId });
                Delete.FromTable("cmsContentVersion").Row(new { ContentId = orphanedId });
                Delete.FromTable("cmsContentXml").Row(new { nodeId = orphanedId });
                Delete.FromTable("cmsContent").Row(new { nodeId = orphanedId });
                Delete.FromTable("umbracoNode").Row(new { id = orphanedId });
            }

            Create.ForeignKey("FK_cmsContent_cmsContentType_nodeId")
                .FromTable("cmsContent")
                .ForeignColumn("contentType")
                .ToTable("cmsContentType")
                .PrimaryColumn("nodeId")
                .OnDelete(Rule.None)
                .OnUpdate(Rule.None);
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7.2 database to a prior version, the database schema has already been modified");
        }
    }
}