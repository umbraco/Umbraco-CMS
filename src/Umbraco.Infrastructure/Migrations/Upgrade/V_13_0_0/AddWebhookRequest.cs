using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class AddWebhookRequest : MigrationBase
{
    public AddWebhookRequest(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.WebhookRequest) is false)
        {
            Create.Table<WebhookRequestDto>().Do();
        }

        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.WebhookRequest);

        LockDto? webhookRequestLock = Database.FirstOrDefault<LockDto>(sql);

        if (webhookRequestLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.WebhookRequest, Name = "WebhookRequest" });
        }
    }
}
