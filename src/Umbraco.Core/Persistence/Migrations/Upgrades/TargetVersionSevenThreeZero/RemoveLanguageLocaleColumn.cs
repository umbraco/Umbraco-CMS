using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 4, GlobalSettings.UmbracoMigrationName)]
    public class RemoveLanguageLocaleColumn : MigrationBase
    {
        public RemoveLanguageLocaleColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();

            if (columns.Any(x => x.ColumnName.InvariantEquals("LanguageLocale") && x.TableName.InvariantEquals("cmsContentVersion")))
            {
                Delete.Column("LanguageLocale").FromTable("cmsContentVersion");
            }
        }

        public override void Down()
        {
        }
    }
}