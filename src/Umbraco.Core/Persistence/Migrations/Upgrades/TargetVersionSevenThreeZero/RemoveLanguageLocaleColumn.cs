using System.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 4, GlobalSettings.UmbracoMigrationName)]
    public class RemoveLanguageLocaleColumn : MigrationBase
    {
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