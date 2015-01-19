using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.0.0", 8, GlobalSettings.UmbracoMigrationName)]
    public class AlterTagRelationsTable : MigrationBase
    {
        public override void Up()
        {
            if (Context == null || Context.Database == null) return;

            Initial();

            Upgrade();

            Final();
        }

        private void Initial()
        {
            var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();

            //create a new col which we will make a foreign key, but first needs to be populated with data.
            Alter.Table("cmsTagRelationship").AddColumn("propertyTypeId").AsInt32().Nullable();

            //drop the foreign key on umbracoNode.  Must drop foreign key first before primary key can be removed in MySql.
            if (Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                Delete.ForeignKey().FromTable("cmsTagRelationship").ForeignColumn("nodeId").ToTable("umbracoNode").PrimaryColumn("id");
            }
            else
            {
                //Before we try to delete this constraint, we'll see if it exists first, some older schemas never had it and some older schema's had this named
                // differently than the default.

                var constraint = constraints
                    .SingleOrDefault(x => x.Item1 == "cmsTagRelationship" && x.Item2 == "nodeId" && x.Item3.InvariantStartsWith("PK_") == false);
                if (constraint != null)
                {
                    Delete.ForeignKey(constraint.Item3).OnTable("cmsTagRelationship");
                }
            }

            //we need to drop the primary key, this is sql specific since MySQL has never had primary keys on this table
            // at least since 6.0 and the new installation way but perhaps it had them way back in 4.x so we need to check
            // it exists before trying to drop it.
            if (Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {   
                //this will let us know if this pk exists on this table
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsTagRelationship") && x.Item3.InvariantEquals("PRIMARY")) > 0)
                {
                    Delete.PrimaryKey("PK_cmsTagRelationship").FromTable("cmsTagRelationship");
                }
            }
            else
            {
                //lookup the PK by name
                var pkName = constraints.FirstOrDefault(x => x.Item1.InvariantEquals("cmsTagRelationship") && x.Item3.InvariantStartsWith("PK_"));
                if (pkName != null)
                {
                    Delete.PrimaryKey(pkName.Item3).FromTable("cmsTagRelationship");    
                }
            }
            
        }

        private void Upgrade()
        {
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
                WHERE cmsDataType.controlId = '4023E540-92F5-11DD-AD8B-0800200C9A66'");

            foreach (var tr in tagRelations)
            {
                //for each tag relation we need to assign it a property type id which must exist in our references, if it doesn't it means that 
                // someone has tag data that relates to node that is not in the cmsContent table - we'll have to delete it and log it if that is the case.

                var propertyTypes = propertyTypeIdRef.Where(x => x.NodeId == tr.NodeId).ToArray();
                if (propertyTypes.Length == 0)
                {
                    Logger.Warn<AlterTagRelationsTable>("There was no cmsContent reference for cmsTagRelationship for nodeId "
                        + tr.NodeId +
                        ". The new tag system only supports tags with references to content in the cmsContent and cmsPropertyType tables. This row will be deleted: "
                        + string.Format("nodeId: {0}, tagId: {1}", tr.NodeId, tr.TagId));
                    Delete.FromTable("cmsTagRelationship").Row(new { nodeId = tr.NodeId, tagId = tr.TagId });
                }
                else
                {
                    //update the first one found to the existing row, there might be more if there are more than one tag property assigned to the node
                    //in that case we need to create a row.

                    //update the table with the alias, the current editorAlias will contain the original id
                    var first = propertyTypes[0];
                    Update.Table("cmsTagRelationship")
                          .Set(new { propertyTypeId = first.PropertyTypeId })
                          .Where(new { nodeId = tr.NodeId, tagId = tr.TagId });

                    if (propertyTypes.Length > 1)
                    {
                        //now we need to create rows for the other ones
                        for (var i = 1; i < propertyTypes.Length; i++)
                        {
                            Insert.IntoTable("cmsTagRelationship").Row(new { nodeId = tr.NodeId, tagId = tr.TagId, propertyTypeId = propertyTypes[i].PropertyTypeId });
                        }
                    }
                }
            }
        }

        private void Final()
        {
            //we need to change this to not nullable
            Alter.Table("cmsTagRelationship").AlterColumn("propertyTypeId").AsInt32().NotNullable();

            //we need to re-add the new primary key on all 3 columns
            Create.PrimaryKey("PK_cmsTagRelationship").OnTable("cmsTagRelationship").Columns(new[] { "nodeId", "propertyTypeId", "tagId" });

            //now we need to add a foreign key to the propertyTypeId column and change it's constraints
            Create.ForeignKey("FK_cmsTagRelationship_cmsPropertyType")
                  .FromTable("cmsTagRelationship")
                  .ForeignColumn("propertyTypeId")
                  .ToTable("cmsPropertyType")
                  .PrimaryColumn("id")
                  .OnDelete(Rule.None)
                  .OnUpdate(Rule.None);

            //now we need to add a foreign key to the nodeId column to cmsContent (intead of the original umbracoNode)
            Create.ForeignKey("FK_cmsTagRelationship_cmsContent")
                  .FromTable("cmsTagRelationship")
                  .ForeignColumn("nodeId")
                  .ToTable("cmsContent")
                  .PrimaryColumn("nodeId")
                  .OnDelete(Rule.None)
                  .OnUpdate(Rule.None);
        }

        public override void Down()
        {
            throw new DataLossException("Cannot downgrade from a version 7 database to a prior version, the database schema has already been modified");
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