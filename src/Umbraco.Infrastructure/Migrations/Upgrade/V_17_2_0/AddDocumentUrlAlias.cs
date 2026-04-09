using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_2_0;

/// <summary>
/// Adds the umbracoDocumentUrlAlias table and its associated lock record.
/// </summary>
public class AddDocumentUrlAlias : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddDocumentUrlAlias"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddDocumentUrlAlias(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.DocumentUrlAlias) is false)
        {
            Create.Table<DocumentUrlAliasDto>().Do();
        }

        // Add lock record for DocumentUrlAliases
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.DocumentUrlAliases);

        LockDto? existingLockDto = Database.FirstOrDefault<LockDto>(sql);
        if (existingLockDto is null)
        {
            Database.Insert(
                Constants.DatabaseSchema.Tables.Lock,
                "id",
                false,
                new LockDto { Id = Constants.Locks.DocumentUrlAliases, Name = "DocumentUrlAliases" });
        }
    }
}
