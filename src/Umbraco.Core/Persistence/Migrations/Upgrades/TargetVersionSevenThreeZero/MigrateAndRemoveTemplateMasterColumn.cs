using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
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

        public MigrateAndRemoveTemplateMasterColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {

            //Don't execute anything if there is no 'master' column - this might occur if the db is already upgraded
            var cols = SqlSyntax.GetColumnsInSchema(Context.Database);
            if (cols.Any(x => x.ColumnName.InvariantEquals("master") && x.TableName.InvariantEquals("cmsTemplate")) == false)
            {
                return;
            }

            //update the parentId column for all templates to be correct so it matches the current 'master' template

            //In some old corrupted databases, the information will not be correct in the master column so we need to fix that
            //first by nulling out the master column where the id doesn't actually exist
            Execute.Sql(@"UPDATE cmsTemplate SET master = NULL WHERE " + 
                SqlSyntax.GetQuotedColumnName("master") + @" IS NOT NULL AND " + 
                SqlSyntax.GetQuotedColumnName("master") + @" NOT IN (SELECT nodeId FROM cmsTemplate)");

            //Now we can bulk update the parentId column

            //NOTE: This single statement should be used but stupid SQLCE doesn't support Update with a FROM !!
            // so now we have to do this by individual rows :(
                    //Execute.Sql(@"UPDATE umbracoNode
                    //SET parentID = COALESCE(t2." + SqlSyntax.GetQuotedColumnName("master")  +  @", -1)
                    //FROM umbracoNode t1
                    //INNER JOIN cmsTemplate t2
                    //ON t1.id = t2.nodeId");
            Execute.Code(database =>
            {
                var templateData = database.Fetch<dynamic>("SELECT * FROM cmsTemplate");

                foreach (var template in templateData)
                {
                    var sql = "SET parentID=@parentId WHERE id=@nodeId";

                    LogHelper.Info<MigrateAndRemoveTemplateMasterColumn>("Executing sql statement: UPDATE umbracoNode " + sql);

                    database.Update<NodeDto>(sql,
                        new {parentId = template.master ?? -1, nodeId = template.nodeId});
                }

                return string.Empty;
            });

            //Now we can update the path, but this needs to be done in a delegate callback so that the query runs after the updates just completed
            Execute.Code(database =>
            {
                //NOTE: we are using dynamic because we need to get the data in a column that no longer exists in the schema
                var templates = database.Fetch<dynamic>(new Sql().Select("*").From<TemplateDto>());
                foreach (var template in templates)
                {
                    var sql = string.Format(SqlSyntax.UpdateData,
                        SqlSyntax.GetQuotedTableName("umbracoNode"),
                        "path=@buildPath",
                        "id=@nodeId");

                    LogHelper.Info<MigrateAndRemoveTemplateMasterColumn>("Executing sql statement: " + sql);

                    //now build the correct path for the template
                    database.Execute(sql, new
                    {
                        buildPath = BuildPath(template, templates),
                        nodeId = template.nodeId
                    });
                }

                return string.Empty;
            });

            

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