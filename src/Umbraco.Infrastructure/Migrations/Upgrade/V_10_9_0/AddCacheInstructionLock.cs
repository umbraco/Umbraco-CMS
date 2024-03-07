using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_9_0;

internal class AddCacheInstructionLock : MigrationBase
{
    public AddCacheInstructionLock(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var currentCount = Database
            .Fetch<int>(
                Sql()
                    .SelectCount()
                    .From<LockDto>()
                    .Where<LockDto>(x => x.Id == Constants.Locks.CacheInstructions))
            .FirstOrDefault();
        if (currentCount == 0)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false,
                new LockDto { Id = Constants.Locks.CacheInstructions, Name = "CacheInstructions" });
        }

    }
}
