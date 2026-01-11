using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
public class Webhook2EventsDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Events;

    private const string WebhookIdColumnName = "webhookId";
    private const string EventColumnName = "event";

    [Column(WebhookIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_webhookEvent2WebhookDto", OnColumns = $"{WebhookIdColumnName}, {EventColumnName}")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column(EventColumnName)]
    public string Event { get; set; } = string.Empty;
}
