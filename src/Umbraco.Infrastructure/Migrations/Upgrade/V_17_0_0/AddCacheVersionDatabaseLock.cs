﻿using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

public class AddCacheVersionDatabaseLock : AsyncMigrationBase
{
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
