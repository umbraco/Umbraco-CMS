using System.Linq;
using System.Web.Security;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Security;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.13.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddSortOrderToDomainTable : MigrationBase
    {
        public AddSortOrderToDomainTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoDomains") && x.ColumnName.InvariantEquals("sortOrder")) == false)
                Create.Column("sortOrder").OnTable("umbracoDomains").AsInt32().Nullable().WithDefault(0);
        }

        public override void Down()
        {
        }
    }
}
