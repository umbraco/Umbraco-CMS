using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 0, Constants.System.UmbracoMigrationName)]
    public class UpdateUserTables : MigrationBase
    {
        public UpdateUserTables(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("createDate")) == false)
                Create.Column("createDate").OnTable("umbracoUser").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("updateDate")) == false)
                Create.Column("updateDate").OnTable("umbracoUser").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("emailConfirmedDate")) == false)
                Create.Column("emailConfirmedDate").OnTable("umbracoUser").AsDateTime().Nullable();
        }

        public override void Down()
        {
        }
    }
}