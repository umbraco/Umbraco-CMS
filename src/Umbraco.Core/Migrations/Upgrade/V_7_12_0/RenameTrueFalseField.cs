using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwelveZero
{
    [Migration("7.12.0", 0, Constants.System.UmbracoMigrationName)]
    public class RenameTrueFalseField : MigrationBase
    {
        public RenameTrueFalseField(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //rename the existing true/false field
            Update.Table("umbracoNode").Set(new { text = "Checkbox" }).Where(new { id = -49 });
        }

        public override void Down()
        {
            //set the field back to true/false
            Update.Table("umbracoNode").Set(new { text = "True/false" }).Where(new { id = -49 });
        }
    }
}
