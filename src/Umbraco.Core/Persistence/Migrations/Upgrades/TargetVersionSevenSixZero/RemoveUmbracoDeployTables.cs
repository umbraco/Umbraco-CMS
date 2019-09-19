using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class RemoveUmbracoDeployTables : MigrationBase
    {
        public RemoveUmbracoDeployTables(ISqlSyntaxProvider sqlSyntax, ILogger logger) 
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            // there are two versions of umbracoDeployDependency,
            // 1. one created by 7.4 and never used, we need to remove it (has a sourceId column)
            // 2. one created by Deploy itself, we need to keep it (has a sourceUdi column)
            if (tables.InvariantContains("umbracoDeployDependency"))
            {
                var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();
                if (columns.Any(x => x.TableName.InvariantEquals("umbracoDeployDependency") && x.ColumnName.InvariantEquals("sourceId")))
                    Delete.Table("umbracoDeployDependency");
            }

            // always remove umbracoDeployChecksum
            if (tables.InvariantContains("umbracoDeployChecksum"))
                Delete.Table("umbracoDeployChecksum");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
