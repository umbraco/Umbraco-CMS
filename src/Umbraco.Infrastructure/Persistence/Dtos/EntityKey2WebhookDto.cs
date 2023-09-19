using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(Constants.DatabaseSchema.Tables.EntityKey2Webhook)]
[ExplicitColumns]
public class EntityKey2WebhookDto
{
    [Column("webhookId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_webhookEntityKey2Webhook", OnColumns = "webhookId, entityKey")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column("entityKey")]
    public Guid EntityKey { get; set; }
}
