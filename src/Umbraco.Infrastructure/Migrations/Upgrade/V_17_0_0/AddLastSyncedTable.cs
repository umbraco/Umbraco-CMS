using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

public class AddLastSyncedTable : AsyncMigrationBase
{
    public AddLastSyncedTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        if (TableExists(LastSyncedDto.TableName) is false)
        {
            Create.Table<LastSyncedDto>().Do();
        }

        return Task.CompletedTask;
    }
}
