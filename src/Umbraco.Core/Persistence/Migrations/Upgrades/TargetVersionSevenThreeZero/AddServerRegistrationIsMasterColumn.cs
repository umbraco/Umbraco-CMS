using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 17, GlobalSettings.UmbracoMigrationName)]
    public class AddServerRegistrationIsMasterColumn : MigrationBase
    {
        public AddServerRegistrationIsMasterColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // don't execute if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();
            if (columns.Any(x => x.TableName.InvariantEquals("umbracoServer") && x.ColumnName.InvariantEquals("isMaster")) == false)
            {
                Create.Column("isMaster").OnTable("umbracoServer").AsBoolean().NotNullable().WithDefaultValue(0);
            }
        }

        public override void Down()
        {
            // not implemented
        }
    }
}
