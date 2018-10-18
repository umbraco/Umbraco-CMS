using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddLogTableColumns : MigrationBase
    {
        public AddLogTableColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.Log) && !x.ColumnName.InvariantEquals("entityType")))
                AddColumn<LogDto>("entityType");

            if (columns.Any(x => x.TableName.InvariantEquals(Constants.DatabaseSchema.Tables.Log) && !x.ColumnName.InvariantEquals("parameters")))
                AddColumn<LogDto>("parameters");
            
        }
    }
}
