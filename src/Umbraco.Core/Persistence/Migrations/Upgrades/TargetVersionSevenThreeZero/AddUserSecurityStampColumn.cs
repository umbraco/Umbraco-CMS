using System.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class AddUserSecurityStampColumn : MigrationBase
    {
        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();
            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("securityStampToken"))) return;

            Create.Column("securityStampToken").OnTable("umbracoUser").AsString(255).Nullable();
        }

        public override void Down()
        {
        }
    }
}