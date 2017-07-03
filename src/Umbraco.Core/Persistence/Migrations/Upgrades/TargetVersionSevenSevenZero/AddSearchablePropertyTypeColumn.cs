using System.Data;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 110, Constants.System.UmbracoMigrationName)]
    public class AddSearchablePropertyTypeColumn : MigrationBase
    {
        public AddSearchablePropertyTypeColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyType") && x.ColumnName.InvariantEquals("Searchable")) == false)
            {
                Create.Column("Searchable").OnTable("cmsPropertyType").AsBoolean().NotNullable().WithDefault(0);
            }
        }

        public override void Down()
        {
            // not implemented
        }
    }
}
