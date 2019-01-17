using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwelveZero
{
    [Migration("7.12.0", 1, Constants.System.UmbracoMigrationName)]
    public class UpdateUmbracoConsent : MigrationBase
    {
        public UpdateUmbracoConsent(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Alter.Table("umbracoConsent").AlterColumn("comment").AsString().Nullable();
        }

        public override void Down()
        {
            // We can't remove this in case we already have null values saved in the column
        }
    }
}
