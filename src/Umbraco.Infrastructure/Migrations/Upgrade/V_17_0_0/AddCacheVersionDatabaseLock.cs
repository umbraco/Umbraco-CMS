using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Migration that introduces a database lock for cache versioning to ensure data consistency during upgrades.
/// </summary>
public class AddCacheVersionDatabaseLock : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddCacheVersionDatabaseLock"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
    public AddCacheVersionDatabaseLock(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.CacheVersion);

        LockDto? cacheVersionLock = Database.Fetch<LockDto>(sql).FirstOrDefault();


        if (cacheVersionLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.CacheVersion, Name = "CacheVersion" });
        }

        return Task.CompletedTask;
    }
}
