using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    [Migration("7.4.0", 4, GlobalSettings.UmbracoMigrationName)]
    public class RemoveParentIdPropertyTypeGroupColumn : MigrationBase
    {
        public RemoveParentIdPropertyTypeGroupColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // don't execute if the column is not there anymore
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyTypeGroup") && x.ColumnName.InvariantEquals("parentGroupId")) == false)
                return;

            //This constraing can be based on old aliases, before removing them, check they exist
            var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();
            if (constraints.Any(x => x.Item1.InvariantEquals("cmsPropertyTypeGroup") && x.Item3.InvariantEquals("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup_id")))
            {
                Delete.ForeignKey("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup_id").OnTable("cmsPropertyTypeGroup");
            }
            if (constraints.Any(x => x.Item1.InvariantEquals("cmsPropertyTypeGroup") && x.Item3.InvariantEquals("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup")))
            {
                Delete.ForeignKey("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup").OnTable("cmsPropertyTypeGroup");
            }
            
            Delete.Column("parentGroupId").FromTable("cmsPropertyTypeGroup");
        }

        public override void Down()
        { }
    }
}
