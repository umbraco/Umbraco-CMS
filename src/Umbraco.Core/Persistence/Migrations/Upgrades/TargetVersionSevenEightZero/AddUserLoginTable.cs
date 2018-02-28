using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenEightZero
{
    [Migration("7.8.0", 4, Constants.System.UmbracoMigrationName)]
    public class AddUserLoginTable : MigrationBase
    {
        public AddUserLoginTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains("umbracoUserLogin") == false)
            {
                Create.Table<UserLoginDto>();
            }
        }
        
        public override void Down()
        {
        }
    }
}