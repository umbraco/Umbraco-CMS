using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 13, GlobalSettings.UmbracoMigrationName)]
    public class AddUniqueIdPropertyTypeColumn : MigrationBase
    {
        public AddUniqueIdPropertyTypeColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyType") && x.ColumnName.InvariantEquals("uniqueID")) == false)
            {
                Create.Column("uniqueID").OnTable("cmsPropertyType").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);

                //unique constraint on name + version
                Create.Index("IX_cmsPropertyTypeUniqueID").OnTable("cmsPropertyType")
                    .OnColumn("uniqueID").Ascending()
                    .WithOptions()
                    .NonClustered()
                    .WithOptions()
                    .Unique();
            }

            
        }

        public override void Down()
        {
        }
    }
}