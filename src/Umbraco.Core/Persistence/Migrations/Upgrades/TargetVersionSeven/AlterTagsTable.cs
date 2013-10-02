using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 8, GlobalSettings.UmbracoMigrationName)]
    public class AlterTagsTableInitial : MigrationBase
    {
        public override void Up()
        {
            //create a new col which we will make a foreign key, but first needs to be populated with data.
            Alter.Table("cmsTagRelationship").AddColumn("propertyTypeId").AsInt32().Nullable();
        }

        public override void Down()
        {
            throw new NotSupportedException();
        }
    }

    [Migration("7.0.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class AlterTagsTableFinal : MigrationBase
    {
        public override void Up()
        {
            //now we need to add a foreign key to the propertyTypeId column and change it's constraints
            Create.ForeignKey("FK_cmsTagRelationship_cmsPropertyType")
                  .FromTable("cmsTagRelationship")
                  .ForeignColumn("propertyTypeId")
                  .ToTable("cmsPropertyType")
                  .PrimaryColumn("id")
                  .OnDelete(Rule.Cascade)
                  .OnUpdate(Rule.None);

            //change the column to be not nullable
            Alter.Table("cmsTagRelationship").AlterColumn("propertyTypeId").AsInt32().NotNullable();

            //now we need to drop the previous foreign key on the umbracoNode table
            Delete.ForeignKey("FK_cmsTagRelationship_umbracoNode_id").OnTable("cmsTagRelationship");

            //now we need to delete the colum
            Delete.Column("nodeId").FromTable("cmsTagRelationship");
        }

        public override void Down()
        {
            throw new NotSupportedException();
        }
    }

    [Migration("7.0.0", 9, GlobalSettings.UmbracoMigrationName)]
    public class UpgradeTagTableData : MigrationBase
    {
        public override void Up()
        {
            if (Context == null || Context.Database == null) return;

            //get all data from the tag relationship table
            var tagRelations = Context.Database.Fetch<TagRelationshipDto>("SELECT nodeId, tagId FROM cmsTagRelationship");

            //get all node id -> property type id references for nodes that are in a tag relations and for properties that are of those nodes that are of the tag data type
            var propertyTypeIdRef = Context.Database.Fetch<PropertyTypeReferenceDto>(@"SELECT DISTINCT cmsTagRelationship.nodeId as NodeId, cmsPropertyType.id as PropertyTypeId
                FROM cmsTags 
                INNER JOIN cmsTagRelationship ON cmsTagRelationship.tagId = cmsTags.id
                INNER JOIN umbracoNode ON umbracoNode.id = cmsTagRelationship.nodeId
                INNER JOIN cmsContent ON cmsContent.nodeId = umbracoNode.id
                INNER JOIN cmsContentType ON cmsContentType.nodeId = cmsContent.contentType
                INNER JOIN cmsPropertyType ON cmsPropertyType.contentTypeId = cmsContentType.nodeId
                INNER JOIN cmsDataType ON cmsDataType.nodeId = cmsPropertyType.dataTypeId
                WHERE cmsDataType.propertyEditorAlias = 'Umbraco.Tags'");

            foreach (var tr in tagRelations)
            {
                //for each tag relation we need to assign it a property type id which must exist in our references, if it doesn't it means that 
                // someone has tag data that relates to node that is not in the cmsContent table - we'll have to delete it and log it if that is the case.

                var propertyTypes = propertyTypeIdRef.Where(x => x.NodeId == tr.NodeId).ToArray();
                if (propertyTypes.Length == 0)
                {
                    LogHelper.Warn<UpgradeTagTableData>("There was no cmsContent reference for cmsTagRelationship for nodeId " + tr.NodeId + ". The new tag system only supports tags with references to content in the cmsContent table. This row will be deleted: " + string.Format("nodeId: {0}, tagId: {1}", tr.NodeId, tr.TagId));
                    Delete.FromTable("cmsTagRelationship").Row(new {nodeId = tr.NodeId, tagId = tr.TagId});
                }
                else
                {
                    //update the first one found to the existing row, there might be more if there are more than one tag property assigned to the node
                    //in that case we need to create a row.

                    //update the table with the alias, the current editorAlias will contain the original id
                    var first = propertyTypes[0];
                    Update.Table("cmsTagRelationship")
                          .Set(new {propertyTypeId = first.PropertyTypeId})
                          .Where(new {nodeId = tr.NodeId, tagId = tr.TagId});

                    if (propertyTypes.Length > 1)
                    {
                        //now we need to create rows for the other ones
                        for (var i = 1; i < propertyTypes.Length; i++)
                        {
                            Insert.IntoTable("cmsTagRelationship").Row(new {nodeId = tr.NodeId, tagId = tr.TagId, propertyTypeId = propertyTypes[i].PropertyTypeId});
                        }
                    }
                }
            }
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from a version 7 database to a prior version");
        }

        /// <summary>
        /// A custom class to map to so that we can linq to it easily without dynamics
        /// </summary>
        private class PropertyTypeReferenceDto
        {
            public int NodeId { get; set; }
            public int PropertyTypeId { get; set; }
        }
    }
}