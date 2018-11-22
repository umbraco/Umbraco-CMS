using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenEightZero
{
    [Migration("7.8.0", 2, Constants.System.UmbracoMigrationName)]
    public class AddInstructionCountColumn : MigrationBase
    {
        public AddInstructionCountColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoCacheInstruction") && x.ColumnName.InvariantEquals("instructionCount")) == false)
                Create.Column("instructionCount")
                    .OnTable("umbracoCacheInstruction")
                    .AsInt32()
                    .WithDefaultValue(1)
                    .NotNullable();
        }

        public override void Down()
        {
        }
    }
}
