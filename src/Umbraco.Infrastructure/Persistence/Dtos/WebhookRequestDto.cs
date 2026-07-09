using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a data transfer object used for persisting webhook request data in the database.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class WebhookRequestDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.WebhookRequest;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the webhook request.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the webhook.
    /// </summary>
    [Column("webhookKey")]
    public Guid WebhookKey { get; set; }

    /// <summary>
    /// Gets or sets the alias that identifies the webhook event.
    /// </summary>
    [Column("eventName")]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized JSON representation of the webhook request object.
    /// </summary>
    [Column(Name = "requestObject")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? RequestObject { get; set; }

    /// <summary>
    /// Gets or sets the number of times the webhook request has been retried.
    /// </summary>
    [Column("retryCount")]
    public int RetryCount { get; set; }
}
