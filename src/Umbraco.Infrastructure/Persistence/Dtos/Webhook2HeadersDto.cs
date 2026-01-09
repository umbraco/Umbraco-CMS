using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
public class Webhook2HeadersDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Headers;
    public const string KeyName = "Key";
    public const string ValueName = "Value";
    public const string WebhookIdName = "WebhookId";

    [Column(WebhookIdName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_headers2WebhookDto", OnColumns = $"{WebhookIdName}, {KeyName}")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column(KeyName)]
    public string Key { get; set; } = string.Empty;

    [Column(ValueName)]
    public string Value { get; set; } = string.Empty;
}
