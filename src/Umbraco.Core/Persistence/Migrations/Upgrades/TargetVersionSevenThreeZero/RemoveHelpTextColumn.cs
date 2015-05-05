using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 8, GlobalSettings.UmbracoMigrationName)]
    public class RemoveHelpTextColumn : MigrationBase
    {
        public RemoveHelpTextColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();

            if (columns.Any(x => x.ColumnName.InvariantEquals("helpText") && x.TableName.InvariantEquals("cmsPropertyType")))
            {
                Delete.Column("helpText").FromTable("cmsPropertyType");
            }
        }

        public override void Down()
        {
        }
    }
}