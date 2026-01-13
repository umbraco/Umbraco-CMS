using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_2_0;

/// <summary>
/// Adds the umbracoDocumentAlias table and its associated lock record.
/// </summary>
public class AddDocumentAlias : MigrationBase
{
    public AddDocumentAlias(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.DocumentAlias) is false)
        {
            Create.Table<DocumentAliasDto>().Do();
        }

        // Add lock record for DocumentAliases
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.DocumentAliases);

        LockDto? existingLockDto = Database.FirstOrDefault<LockDto>(sql);
        if (existingLockDto is null)
        {
            Database.Insert(
                Constants.DatabaseSchema.Tables.Lock,
                "id",
                false,
                new LockDto { Id = Constants.Locks.DocumentAliases, Name = "DocumentAliases" });
        }
    }
}
