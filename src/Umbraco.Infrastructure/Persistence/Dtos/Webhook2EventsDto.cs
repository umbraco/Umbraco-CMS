using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object that maps webhooks to their associated events.
/// </summary>
[TableName(TableName)]
[PrimaryKey([WebhookIdColumnName, EventColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class Webhook2EventsDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Events;

    private const string WebhookIdColumnName = "webhookId";
    private const string EventColumnName = "event";

    /// <summary>
    /// Gets or sets the identifier of the webhook associated with this event.
    /// </summary>
    [Column(WebhookIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_webhookEvent2WebhookDto", OnColumns = $"{WebhookIdColumnName}, {EventColumnName}")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    /// <summary>
    /// Gets or sets the name of the webhook event.
    /// </summary>
    [Column(EventColumnName)]
    public string Event { get; set; } = string.Empty;
}
