using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_1_0
{
    public class FixContentNuCascade : MigrationBase
    {
        public FixContentNuCascade(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Delete.KeysAndIndexes<ContentNuDto>().Do();
            Create.KeysAndIndexes<ContentNuDto>().Do();
        }
    }
}
