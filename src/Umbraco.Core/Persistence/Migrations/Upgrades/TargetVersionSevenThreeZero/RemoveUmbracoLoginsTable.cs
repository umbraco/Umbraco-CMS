using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 15, GlobalSettings.UmbracoMigrationName)]
    public class RemoveUmbracoLoginsTable : MigrationBase
    {
        public RemoveUmbracoLoginsTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            if (SqlSyntax.GetColumnsInSchema(Context.Database).Any(x => x.TableName.InvariantEquals("umbracoUserLogins")))
            {
                Delete.Table("umbracoUserLogins");                
            }
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}