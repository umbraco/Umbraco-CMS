using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 100, Constants.System.UmbracoMigrationName)]
    public class AddTwoFactorAuthenticationTable : MigrationBase
    {
        public AddTwoFactorAuthenticationTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            var tableName = "umbracoTwoFactorAuthentication";

            if (tables.InvariantContains(tableName) == false)
            {
                Create.Table(tableName)
                    .WithColumn("userId").AsInt32().NotNullable()
                    .WithColumn("key").AsString()
                    .WithColumn("value").AsString()
                    .WithColumn("confirmed").AsBoolean();
            }
        }

        public override void Down()
        {
            // not implemented
        }
    }
}
