using Umbraco.Cms.Core;
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

    protected override void Migrate()
    {
        LockDto? existingDto = Database.Single<LockDto>(Context.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(dto => dto.Id == Constants.Locks.DocumentUrls));

        if (existingDto is not null)
        {
            return;
        }

        Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.DocumentUrls, Name = "DocumentUrls" });
    }
}
