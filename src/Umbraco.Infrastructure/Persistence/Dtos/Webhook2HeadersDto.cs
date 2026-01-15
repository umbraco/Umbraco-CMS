using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
public class Webhook2HeadersDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Headers;

    internal const string ReferenceMemberName = "WebhookId"; // should be WebhookIdColumnName, but for database compatibility we keep it like this

    private const string WebhookIdColumnName = "webhookId";
    private const string KeyColumnName = "Key";
    private const string ValueColumnName = "Value";

    [Column(WebhookIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_headers2WebhookDto", OnColumns = $"{WebhookIdColumnName}, {KeyColumnName}")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    [Column(KeyColumnName)]
    public string Key { get; set; } = string.Empty;

    [Column(ValueColumnName)]
    public string Value { get; set; } = string.Empty;
}
