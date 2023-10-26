using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class AddWebhooks : MigrationBase
{
    public AddWebhooks(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook))
        {
            return;
        }

        Create.Table<WebhookDto>().Do();

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Event2Webhook))
        {
            return;
        }

        Create.Table<Event2WebhookDto>().Do();

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.EntityKey2Webhook))
        {
            return;
        }

        Create.Table<EntityKey2WebhookDto>().Do();

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Headers2Webhook))
        {
            return;
        }

        Create.Table<Headers2WebhookDto>().Do();

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.WebhookLog))
        {
            return;
        }

        Create.Table<WebhookLogDto>().Do();
    }
}
