using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

/// <summary>
/// Represents a migration step that adds the <c>ExceptionOccured</c> column to the relevant database table as part of the upgrade to version 13.0.0.
/// </summary>
public class AddExceptionOccured : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0.AddExceptionOccured"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration.</param>
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
