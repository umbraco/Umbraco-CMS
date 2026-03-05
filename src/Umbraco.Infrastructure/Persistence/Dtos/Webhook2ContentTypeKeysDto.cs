using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(TableName)]
[PrimaryKey([WebhookIdColumnName, ContentTypeKeyColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class Webhook2ContentTypeKeysDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2ContentTypeKeys;

    private const string WebhookIdColumnName = "webhookId";
    private const string ContentTypeKeyColumnName = "entityKey";

    [Column(WebhookIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_webhookEntityKey2Webhook", OnColumns = $"{WebhookIdColumnName}, {ContentTypeKeyColumnName}")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column(ContentTypeKeyColumnName)]
    public Guid ContentTypeKey { get; set; }
}
