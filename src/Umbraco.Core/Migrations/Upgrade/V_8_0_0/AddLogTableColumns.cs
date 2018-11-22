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

            AddColumnIfNotExists<LogDto>(columns, "entityType");
            AddColumnIfNotExists<LogDto>(columns, "parameters");            
        }
    }
}
