using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_1_0
{
    public class AddLastCacheInstructionColumn : MigrationBase
    {
        public AddLastCacheInstructionColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<ServerRegistrationDto>(columns, "lastCacheInstructionId");
        }
    }
}
