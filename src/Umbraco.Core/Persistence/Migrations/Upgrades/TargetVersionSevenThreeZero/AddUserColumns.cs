using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class AddUserColumns : MigrationBase
    {
        public AddUserColumns(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("securityStampToken")) == false)
                Create.Column("securityStampToken").OnTable("umbracoUser").AsString(255).Nullable();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("failedLoginAttempts")) == false)
                Create.Column("failedLoginAttempts").OnTable("umbracoUser").AsInt32().Nullable();
            
            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("lastLockoutDate")) == false)
                Create.Column("lastLockoutDate").OnTable("umbracoUser").AsDateTime().Nullable();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("lastPasswordChangeDate")) == false)
                Create.Column("lastPasswordChangeDate").OnTable("umbracoUser").AsDateTime().Nullable();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("lastLoginDate")) == false);
                Create.Column("lastLoginDate").OnTable("umbracoUser").AsDateTime().Nullable();
        }

        public override void Down()
        {
        }
    }
}