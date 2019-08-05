using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_2_0
{
    public class AddLockTableColumns : MigrationBase
    {
        public AddLockTableColumns(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<LockDto>(columns, "writeLockReasonId");
        }
    }
}
