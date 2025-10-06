using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Adds all the distributed jobs to the database.
/// </summary>
public class AddDistributedJobs : AsyncMigrationBase
{
    private readonly IEnumerable<IDistributedBackgroundJob> _distributedBackgroundJobs;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDistributedJobs"/> class.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="distributedBackgroundJobs"></param>
    public AddDistributedJobs(IMigrationContext context, IEnumerable<IDistributedBackgroundJob> distributedBackgroundJobs)
        : base(context) => _distributedBackgroundJobs = distributedBackgroundJobs;

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (!TableExists(Constants.DatabaseSchema.Tables.DistributedJob))
        {
            Create.Table<DistributedJobDto>().Do();
        }

        AddDistributedJobLock();

        foreach (IDistributedBackgroundJob distributedJob in _distributedBackgroundJobs)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.DistributedJob, "id", true, new DistributedJobDto { Name = distributedJob.Name, Period = distributedJob.Period.Ticks, LastRun = DateTime.UtcNow, IsRunning = false, LastAttemptedRun = DateTime.UtcNow});
        }

        return Task.CompletedTask;
    }

    private void AddDistributedJobLock()
    {
        if (!TableExists(Constants.DatabaseSchema.Tables.Lock))
        {
            Create.Table<LockDto>().Do();
        }

        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.DistributedJobs);

        LockDto? existingLockDto = Database.FirstOrDefault<LockDto>(sql);
        if (existingLockDto is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.DistributedJobs, Name = "DistributedJobs" });
        }
    }
}
