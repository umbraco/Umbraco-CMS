using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_8_0
{
    internal class AddInstructionCountColumn : MigrationBase
    {
        public AddInstructionCountColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.CacheInstruction) && x.ColumnName.InvariantEquals("instructionCount")) == false)
                AddColumn<CacheInstructionDto>("instructionCount");
        }
    }
}
