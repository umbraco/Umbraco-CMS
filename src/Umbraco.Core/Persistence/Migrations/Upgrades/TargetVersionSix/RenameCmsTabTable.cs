using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class RenameCmsTabTable : MigrationBase
    {
        public RenameCmsTabTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Rename.Table("cmsTab").To("cmsPropertyTypeGroup");
        }

        public override void Down()
        {
            Rename.Table("cmsPropertyTypeGroup").To("cmsTab");
        }
    }
}