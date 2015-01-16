using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    /// <summary>
    /// Remove the master column after we've migrated all of the values into the 'ParentId' and Path column of Umbraco node
    /// </summary>
    [Migration("7.3.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class MigrateAndRemoveTemplateMasterColumn : MigrationBase
    {
        public override void Up()
        {

            //Don't execute anything if there is no 'master' column - this might occur if the db is already upgraded
            var cols = SqlSyntax.GetColumnsInSchema(Context.Database);
            if (cols.Any(x => x.ColumnName.InvariantEquals("master") && x.TableName.InvariantEquals("cmsTemplate")) == false)
            {
                return;
            }

            //update the parentId column for all templates to be correct so it matches the current 'master' template
            //NOTE: we are using dynamic because we need to get the data in a column that no longer exists in the schema
            var templates = Context.Database.Fetch<dynamic>(new Sql().Select("*").From<TemplateDto>());
            foreach (var template in templates)
            {
                Update.Table("umbracoNode").Set(new {parentID = template.master ?? -1}).Where(new {id = template.nodeId});

                //now build the correct path for the template
                Update.Table("umbracoNode").Set(new { path = BuildPath (template, templates)}).Where(new { id = template.nodeId });
                
            }

            //now remove the master column and key
            if (this.Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                Delete.ForeignKey().FromTable("cmsTemplate").ForeignColumn("master").ToTable("umbracoUser").PrimaryColumn("id");
            }
            else
            {
                //These are the old aliases, before removing them, check they exist
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();

                if (constraints.Any(x => x.Item1.InvariantEquals("cmsTemplate") && x.Item3.InvariantEquals("FK_cmsTemplate_cmsTemplate")))
                {
                    Delete.ForeignKey("FK_cmsTemplate_cmsTemplate").OnTable("cmsTemplate");                   
                }

                //TODO: Hopefully it's not named something else silly in some crazy old versions
            }

            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            if (columns.Any(x => x.ColumnName.InvariantEquals("master") && x.TableName.InvariantEquals("cmsTemplate")))
            {
                Delete.Column("master").FromTable("cmsTemplate");    
            }
        }

        public override void Down()
        {
        }

        private string BuildPath(dynamic template, IEnumerable<dynamic> allTemplates)
        {
            if (template.master == null)
            {
                return string.Format("-1,{0}", template.nodeId);
            }

            var parent = allTemplates.FirstOrDefault(x => x.nodeId == template.master);

            if (parent == null)
            {
                //this shouldn't happen but i suppose it could if people have bad data
                return string.Format("-1,{0}", template.nodeId);
            }

            //recurse
            var parentPath = BuildPath(parent, allTemplates);

            var path = parentPath + string.Format(",{0}", template.nodeId);
            return path;
        }
    }
}