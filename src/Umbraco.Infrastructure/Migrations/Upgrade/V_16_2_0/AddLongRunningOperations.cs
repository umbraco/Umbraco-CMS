using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_2_0;

/// <summary>
/// Represents a database migration that adds support for tracking and managing long-running operations.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class AddLongRunningOperations : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLongRunningOperations"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
    public AddLongRunningOperations(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (!TableExists(Constants.DatabaseSchema.Tables.LongRunningOperation))
        {
            Create.Table<LongRunningOperationDto>().Do();
        }

        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.LongRunningOperations);

        LockDto? existingLockDto = Database.FirstOrDefault<LockDto>(sql);
        if (existingLockDto is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.LongRunningOperations, Name = "LongRunningOperations" });
        }
    }
}
