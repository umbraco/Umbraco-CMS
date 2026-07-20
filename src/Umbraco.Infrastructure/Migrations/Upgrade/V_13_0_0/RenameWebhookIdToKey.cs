using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

/// <summary>
/// Represents a migration that renames the <c>WebhookId</c> column to <c>Key</c> in the relevant database table as part of the upgrade to version 13.0.0.
/// </summary>
public class RenameWebhookIdToKey : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameWebhookIdToKey"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for migration operations.</param>
    public RenameWebhookIdToKey(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        // This check is here because we renamed a column from 13-rc1 to 13-rc2, the previous migration adds the table
        // so if you are upgrading from 13-rc1 to 13-rc2 then this column will not exist.
        // If you are however upgrading from 12, then this column will exist, and thus there is no need to rename it.
        if (ColumnExists(Constants.DatabaseSchema.Tables.WebhookLog, "webhookId") is false)
        {
            return;
        }

        Rename
            .Column("webhookId")
            .OnTable(Constants.DatabaseSchema.Tables.WebhookLog)
            .To("webhookKey")
            .Do();
    }
}
