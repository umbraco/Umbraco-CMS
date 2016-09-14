using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class AddValidationMessagesForPropertyTypes : MigrationBase
    {
        public AddValidationMessagesForPropertyTypes(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyType") && x.ColumnName.InvariantEquals("mandatoryMessage")) == false)
                Create.Column("mandatoryMessage").OnTable("cmsPropertyType").AsString(500).Nullable();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyType") && x.ColumnName.InvariantEquals("validationRegExpMessage")) == false)
                Create.Column("validationRegExpMessage").OnTable("cmsPropertyType").AsString(500).Nullable();
        }

        public override void Down()
        {
        }
    }
}