using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveFive
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-4196
    /// </summary>
    [Migration("7.5.5", 1, Constants.System.UmbracoMigrationName)]
    public class UpdateAllowedMediaTypesAtRoot : MigrationBase
    {
        public UpdateAllowedMediaTypesAtRoot(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            Execute.Sql("UPDATE cmsContentType SET allowAtRoot = 1 WHERE nodeId = 1032 OR nodeId = 1033");
        }

        public override void Down()
        { }
    }
}
