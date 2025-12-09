using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

public class AddElements : AsyncMigrationBase
{
    public AddElements(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        EnsureElementTreeLock();
        EnsureElementTables();
        return Task.CompletedTask;
    }

    private void EnsureElementTreeLock()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.ElementTree);

        LockDto? cacheVersionLock = Database.Fetch<LockDto>(sql).FirstOrDefault();

        if (cacheVersionLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.ElementTree, Name = "ElementTree" });
        }
    }

    private void EnsureElementTables()
    {
        if (!TableExists(Constants.DatabaseSchema.Tables.Element))
        {
            Create.Table<ElementDto>().Do();
        }

        if (!TableExists(Constants.DatabaseSchema.Tables.ElementVersion))
        {
            Create.Table<ElementVersionDto>().Do();
        }

        if (!TableExists(Constants.DatabaseSchema.Tables.ElementCultureVariation))
        {
            Create.Table<ElementCultureVariationDto>().Do();
        }
    }
}
