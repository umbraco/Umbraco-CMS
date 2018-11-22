using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenEightZero
{
    [Migration("7.8.0", 1, Constants.System.UmbracoMigrationName)]
    public class AddTourDataUserColumn : MigrationBase
    {
        public AddTourDataUserColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoUser") && x.ColumnName.InvariantEquals("tourData")) == false)
            {
                var textType = SqlSyntax.GetSpecialDbType(SpecialDbTypes.NTEXT);
                Create.Column("tourData").OnTable("umbracoUser").AsCustom(textType).Nullable();
            }
                
        }

        public override void Down()
        {
        }
    }
    
}
