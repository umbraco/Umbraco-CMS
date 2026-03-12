using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Migration that creates the <c>LastSynced</c> table in the database as part of the upgrade to version 17.0.0.
/// </summary>
public class AddLastSyncedTable : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLastSyncedTable"/> class to create the migration for adding the LastSynced table.
    /// </summary>
    /// <param name="context">The migration context used for the operation.</param>
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
