using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Adds the database lock used to serialize relation writes.
/// </summary>
/// <remarks>
/// This has to run as a pre-migration. Taking a lock whose row is missing throws, and migrations from earlier
/// versions save content, which persists relations, so the row must exist before the main migration plan runs.
/// </remarks>
public class AddRelationsLock : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRelationsLock"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddRelationsLock(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.Relations);

        LockDto? relationsLock = Database.Fetch<LockDto>(sql).FirstOrDefault();

        if (relationsLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.Relations, Name = "Relations" });
        }

        return Task.CompletedTask;
    }
}
