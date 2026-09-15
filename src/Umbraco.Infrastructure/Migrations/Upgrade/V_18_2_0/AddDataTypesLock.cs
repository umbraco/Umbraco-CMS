using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_2_0;

/// <summary>
/// Adds the database lock used to serialize data type writes.
/// </summary>
/// <remarks>
/// This has to run as a pre-migration. Taking a lock whose row is missing throws, and migrations from earlier
/// versions write data types through <see cref="Core.Services.IDataTypeService"/>, so the row must exist before
/// the main migration plan runs.
/// </remarks>
public class AddDataTypesLock : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddDataTypesLock"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddDataTypesLock(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.DataTypes);

        LockDto? dataTypesLock = Database.Fetch<LockDto>(sql).FirstOrDefault();

        if (dataTypesLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.DataTypes, Name = "DataTypes" });
        }

        return Task.CompletedTask;
    }
}
