using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    [Migration("7.4.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class AddDataDecimalColumn : MigrationBase
    {
        public AddDataDecimalColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyData") && x.ColumnName.InvariantEquals("dataDecimal")) == false)
                Create.Column("dataDecimal").OnTable("cmsPropertyData").AsDecimal().Nullable();
        }

        public override void Down()
        {
        }
    }
}