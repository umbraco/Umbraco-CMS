using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_1_0;

public class FixContentNuCascade : MigrationBase
{
    public FixContentNuCascade(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        Delete.KeysAndIndexes<ContentNuDto>().Do();
        Create.KeysAndIndexes<ContentNuDto>().Do();
    }
}
