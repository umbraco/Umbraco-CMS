using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_2_0;

[Obsolete("Remove in Umbraco 18.")]
public class AddLongRunningOperations : MigrationBase
{
    public AddLongRunningOperations(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.LongRunningOperation))
        {
            return;
        }

        Create.Table<LongRunningOperationDto>().Do();
    }
}
