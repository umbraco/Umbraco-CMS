using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

/// <summary>
/// Represents a migration that adds support for database locking for webhooks in Umbraco CMS version 13.0.0.
/// </summary>
public class AddWebhookDatabaseLock : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWebhookDatabaseLock"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration.</param>
    public AddWebhookDatabaseLock(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LockDto>()
            .From<LockDto>()
            .Where<LockDto>(x => x.Id == Constants.Locks.WebhookLogs);

        LockDto? webhookLogsLock = Database.FirstOrDefault<LockDto>(sql);

        if (webhookLogsLock is null)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Lock, "id", false, new LockDto { Id = Constants.Locks.WebhookLogs, Name = "WebhookLogs" });
        }
    }
}
