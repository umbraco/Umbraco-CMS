using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class AddExceptionOccured : MigrationBase
{
    public AddExceptionOccured(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.WebhookLog, "exceptionOccured") == false)
        {
            // Use a custom SQL query to prevent selecting explicit columns (sortOrder doesn't exist yet)
            List<WebhookLogDto> webhookLogDtos = Database.Fetch<WebhookLogDto>($"SELECT * FROM {Constants.DatabaseSchema.Tables.WebhookLog}");

            Delete.Table(Constants.DatabaseSchema.Tables.WebhookLog).Do();
            Create.Table<WebhookLogDto>().Do();

            Database.InsertBatch(webhookLogDtos);
        }
    }
}
