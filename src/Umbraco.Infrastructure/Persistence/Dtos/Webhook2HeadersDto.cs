using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
public class Webhook2HeadersDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Headers;

    [Column("webhookId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_headers2WebhookDto", OnColumns = "webhookId, key")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("value")]
    public string Value { get; set; } = string.Empty;
}
