using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFifteenFour
{
    [Migration("7.15.4", 1, Constants.System.UmbracoMigrationName)]
    public class PopulateMissingSecurityStamps : MigrationBase
    {
        public PopulateMissingSecurityStamps(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            // A user with a NULL securityStampToken can't log in after v7.8.0
            Execute.Sql(@"UPDATE umbracoUser SET securityStampToken = NEWID() WHERE securityStampToken IS NULL");
        }

        public override void Down()
        {
        }
    }
}
