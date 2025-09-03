using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_2_0;

[Obsolete("Remove in Umbraco 18.")]
internal class AddDocumentUrlLock : MigrationBase
{
    public AddDocumentUrlLock(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate() => CreateDocumentUrlsLock(Database);

    /// <summary>
    /// Creates the DocumentUrls lock if it does not already exist.
    /// </summary>
    /// <param name="database">The <see cref="IUmbracoDatabase"/> associated with the migration scope.</param>
    /// <remarks>
    /// Exposed internally to allow for calls from other migration steps that precede this one but require the lock to be in place.
    /// </remarks>
    internal static void CreateDocumentUrlsLock(IUmbracoDatabase database)
    {
        Sql<ISqlContext> sql = database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.DocumentUrls);

        LockDto? existingLockDto = database.FirstOrDefault<LockDto>(sql);
        if (existingLockDto is null)
        {
            database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.DocumentUrls, Name = "DocumentUrls" });
        }
    }
}
