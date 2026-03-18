using System.Data;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object (DTO) for storing header information associated with Webhook2 configurations.
/// This DTO is typically used for persisting or transferring webhook header data within the Umbraco CMS infrastructure.
/// </summary>
[TableName(TableName)]
[PrimaryKey([WebhookIdColumnName, KeyColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class Webhook2HeadersDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Headers;

    private const string WebhookIdColumnName = "webhookId";
    private const string KeyColumnName = "Key";
    private const string ValueColumnName = "Value";

    /// <summary>
    /// Gets or sets the identifier of the webhook associated with this header.
    /// </summary>
    [Column(WebhookIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_headers2WebhookDto", OnColumns = $"{WebhookIdColumnName}, {KeyColumnName}")]
    [ForeignKey(typeof(WebhookDto), OnDelete = Rule.Cascade)]
    public int WebhookId { get; set; }

    /// <summary>
    /// Gets or sets the key of the webhook header.
    /// </summary>
    [Column(KeyColumnName)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value associated with the webhook header.
    /// </summary>
    [Column(ValueColumnName)]
    public string Value { get; set; } = string.Empty;
}
