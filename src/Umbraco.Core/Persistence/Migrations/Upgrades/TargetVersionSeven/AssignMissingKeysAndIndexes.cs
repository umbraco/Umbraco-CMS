using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    /// <summary>
    /// I'm not actually sure how this is possible but I've come across one install that was missing these PKs
    /// and it wasn't a MySQL install. 
    /// see: http://issues.umbraco.org/issue/U4-5707
    /// </summary>
    [Migration("7.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AssignMissingKeysAndIndexes : MigrationBase
    {
        public override void Up()
        {

            //Some very old schemas don't have an index on the cmsContent.nodeId column, I'm not actually sure when it was added but 
            // it is absolutely required to exist in order to have it as a foreign key reference, so we'll need to check it's existence
            // this came to light from this issue: http://issues.umbraco.org/issue/U4-4133
            var dbIndexes = SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsContent")) == false)
            {
                Create.Index("IX_cmsContent").OnTable("cmsContent").OnColumn("nodeId").Ascending().WithOptions().Unique();
            }

            if (Context.CurrentDatabaseProvider == DatabaseProviders.SqlServer 
                || Context.CurrentDatabaseProvider == DatabaseProviders.SqlServerCE)
            {
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();

                //This should be 2 because this table has 2 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsPreviewXml") && x.Item3.InvariantStartsWith("PK_")) == 0)
                {
                    Create.PrimaryKey("PK_cmsContentPreviewXml")
                        .OnTable("cmsPreviewXml")
                        .Columns(new[] { "nodeId", "versionId" });
                }

                if (constraints.Count(x => x.Item1.InvariantEquals("cmsTags") && x.Item3.InvariantStartsWith("PK_")) == 0)
                {
                    Create.PrimaryKey("PK_cmsTags")
                        .OnTable("cmsTags")
                        .Columns(new[] { "id" });
                }

                if (constraints.Count(x => x.Item1.InvariantEquals("cmsStylesheetProperty") && x.Item3.InvariantStartsWith("PK_")) == 0)
                {
                    Create.PrimaryKey("PK_cmsStylesheetProperty")
                        .OnTable("cmsStylesheetProperty")
                        .Columns(new[] { "nodeId" });
                }

                if (constraints.Count(x => x.Item1.InvariantEquals("cmsStylesheet") && x.Item3.InvariantStartsWith("PK_")) == 0)
                {
                    Create.PrimaryKey("PK_cmsStylesheet")
                        .OnTable("cmsStylesheet")
                        .Columns(new[] { "nodeId" });

                    Create.ForeignKey("FK_cmsStylesheet_umbracoNode_id").FromTable("cmsStylesheet").ForeignColumn("nodeId")
                        .ToTable("umbracoNode").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
                }

                if (constraints.Count(x => x.Item1.InvariantEquals("cmsMember") && x.Item3.InvariantStartsWith("PK_")) == 0)
                {
                    Create.PrimaryKey("PK_cmsMember")
                        .OnTable("cmsMember")
                        .Columns(new[] { "nodeId" });

                    Create.ForeignKey("FK_cmsMember_umbracoNode_id").FromTable("cmsMember").ForeignColumn("nodeId")
                        .ToTable("umbracoNode").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);

                    Create.ForeignKey("FK_cmsMember_cmsContent_nodeId").FromTable("cmsMember").ForeignColumn("nodeId")
                        .ToTable("cmsContent").PrimaryColumn("nodeId").OnDeleteOrUpdate(Rule.None);
                }
            }
        }

        public override void Down()
        {
            //don't do anything, these keys should have always existed!
        }
    }
}