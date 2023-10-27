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

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook2Events))
        {
            return;
        }

        Create.Table<Webhook2EventsDto>().Do();

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook2ContentTypeKeys))
        {
            return;
        }

        Create.Table<Webhook2ContentTypeKeysDto>().Do();

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook2Headers))
        {
            return;
        }

        Create.Table<Webhook2HeadersDto>().Do();

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.WebhookLog))
        {
            return;
        }

        Create.Table<WebhookLogDto>().Do();
    }
}
