using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_4_0;

internal class AddScheduledPublishingLock : MigrationBase
{
    public AddScheduledPublishingLock(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() =>
        Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.ScheduledPublishing, Name = "ScheduledPublishing" });
}
