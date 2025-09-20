using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(Constants.DatabaseSchema.Tables.Webhook2ContentTypeKeys)]
[ExplicitColumns]
public class Webhook2ContentTypeKeysDto
{
    [Column("webhookId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_webhookEntityKey2Webhook", OnColumns = "webhookId, entityKey")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column("entityKey")]
    public Guid ContentTypeKey { get; set; }
}
