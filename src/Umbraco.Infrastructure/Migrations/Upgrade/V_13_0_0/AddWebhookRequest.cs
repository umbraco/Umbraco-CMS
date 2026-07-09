using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

/// <summary>
/// Represents a migration that adds the <c>WebhookRequest</c> table to the Umbraco database schema during the upgrade to version 13.0.0.
/// </summary>
public class AddWebhookRequest : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWebhookRequest"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
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
