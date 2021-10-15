using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_18_0
{
    class AddContentVersionCleanupFeature : MigrationBase
    {
        public AddContentVersionCleanupFeature(IMigrationContext context)
            : base(context) { }

        public override void Migrate()
        {
            Create.Table<ContentVersionCleanupPolicyDto>().Do();

            // What's this about, we worry someone else edited table with same change?
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
            AddColumnIfNotExists<ContentVersionDto>(columns, "preventCleanup");
        }
    }
}
