using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Webhook2Events)]
public class Webhook2EventsDto
{
    [Column("webhookId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_webhookEvent2WebhookDto", OnColumns = "webhookId, event")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column("event")]
    public string Event { get; set; } = string.Empty;
}
