using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_3_0;

/// <summary>
/// Represents a migration that adds <c>Name</c> and <c>Description</c> fields to webhook entities in the database.
/// </summary>
public class AddNameAndDescriptionToWebhooks : MigrationBase
{
    private readonly ILogger<AddNameAndDescriptionToWebhooks> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddNameAndDescriptionToWebhooks"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="logger">The logger instance.</param>
    public AddNameAndDescriptionToWebhooks(IMigrationContext context, ILogger<AddNameAndDescriptionToWebhooks> logger)
        : base(context)
    {
        _logger = logger;
    }

    protected override void Migrate()
    {
        Logger.LogDebug("Adding name and description columns to webhooks.");

        if (TableExists(Constants.DatabaseSchema.Tables.Webhook))
        {
            var columns = Context.SqlContext.SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumn(columns, "name");
            AddColumn(columns, "description");
        }
        else
        {
            Logger.LogWarning($"Table {Constants.DatabaseSchema.Tables.Webhook} does not exist so the addition of the name and description by columns in migration {nameof(AddNameAndDescriptionToWebhooks)} cannot be completed.");
        }
    }

    private void AddColumn(List<Persistence.SqlSyntax.ColumnInfo> columns, string column)
    {
        if (columns
            .SingleOrDefault(x => x.TableName == Constants.DatabaseSchema.Tables.Webhook && x.ColumnName == column) is null)
        {
            AddColumn<WebhookDto>(Constants.DatabaseSchema.Tables.Webhook, column);
        }
    }
}
