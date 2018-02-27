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
    [Migration("7.9.0", 1, Constants.System.UmbracoMigrationName)]
    public class AddIsSensitiveMemberTypeColumn : MigrationBase
    {
        public AddIsSensitiveMemberTypeColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsMemberType") && x.ColumnName.InvariantEquals("isSensitive")) == false)
            {
                Create.Column("isSensitive").OnTable("cmsMemberType").AsBoolean().WithDefaultValue(0).NotNullable();
            }
        }

        public override void Down()
        {
        }
    }
}
