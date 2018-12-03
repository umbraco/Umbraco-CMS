using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class RemoveUmbracoDeployTables : MigrationBase
    {
        public RemoveUmbracoDeployTables(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            // there are two versions of umbracoDeployDependency,
            // 1. one created by 7.4 and never used, we need to remove it (has a sourceId column)
            // 2. one created by Deploy itself, we need to keep it (has a sourceUdi column)
            if (tables.InvariantContains("umbracoDeployDependency"))
            {
                var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();
                if (columns.Any(x => x.TableName.InvariantEquals("umbracoDeployDependency") && x.ColumnName.InvariantEquals("sourceId")))
                    Delete.Table("umbracoDeployDependency").Do();
            }

            // always remove umbracoDeployChecksum
            if (tables.InvariantContains("umbracoDeployChecksum"))
                Delete.Table("umbracoDeployChecksum").Do();
        }
    }
}
