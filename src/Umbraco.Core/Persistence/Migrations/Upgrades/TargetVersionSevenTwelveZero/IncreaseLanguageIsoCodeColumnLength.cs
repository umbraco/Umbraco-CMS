using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwelveZero
{
    [Migration("7.12.0", 2, Constants.System.UmbracoMigrationName)]
    public class IncreaseLanguageIsoCodeColumnLength : MigrationBase
    {
        public IncreaseLanguageIsoCodeColumnLength(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(database =>
            {
                database.Execute("DROP INDEX [umbracoLanguage].[IX_umbracoLanguage_languageISOCode]");
                return null;
            });

            Alter.Table("umbracoLanguage")
                .AlterColumn("languageISOCode")
                .AsString(14)
                .Nullable();

            Create.Index("IX_umbracoLanguage_languageISOCode")
                .OnTable("umbracoLanguage")
                .OnColumn("languageISOCode")
                .Unique();
        }

        public override void Down()
        {
        }
    }
}
