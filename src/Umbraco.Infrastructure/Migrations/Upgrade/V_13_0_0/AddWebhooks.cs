using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

/// <summary>
/// Represents a migration that adds webhook support during the upgrade to Umbraco version 13.0.0.
/// </summary>
public class AddWebhooks : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWebhooks"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
    public AddWebhooks(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook) is false)
        {
            Create.Table<WebhookDto>().Do();
        }

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook2Events) is false)
        {
            Create.Table<Webhook2EventsDto>().Do();
        }

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook2ContentTypeKeys) is false)
        {
            Create.Table<Webhook2ContentTypeKeysDto>().Do();
        }

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.Webhook2Headers) is false)
        {
            Create.Table<Webhook2HeadersDto>().Do();
        }

        if (tables.InvariantContains(Constants.DatabaseSchema.Tables.WebhookLog) is false)
        {
            Create.Table<WebhookLogDto>().Do();
        }
    }
}
