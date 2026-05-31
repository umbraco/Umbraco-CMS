using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class WebhookLogDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.WebhookLog;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the webhook log entry.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the webhook associated with this log entry.
    /// </summary>
    [Column("webhookKey")]
    public Guid WebhookKey { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this webhook log entry.
    /// </summary>
    [Column(Name = "key")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code returned by the webhook.
    /// </summary>
    [Column(Name = "statusCode")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string StatusCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the webhook log entry was created.
    /// </summary>
    [Column(Name = "date")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + Constants.DatabaseSchema.Tables.WebhookLog + "_date")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the URL associated with the webhook log entry.
    /// </summary>
    [Column(Name = "url")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event alias associated with this webhook log entry.
    /// </summary>
    [Column(Name = "eventAlias")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string EventAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of times the webhook has been retried.
    /// </summary>
    [Column(Name = "retryCount")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the HTTP request headers associated with the webhook log entry.
    /// </summary>
    [Column(Name = "requestHeaders")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string RequestHeaders { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request body content of the webhook.
    /// </summary>
    [Column(Name = "requestBody")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string RequestBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response headers from the webhook call.
    /// </summary>
    [Column(Name = "responseHeaders")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ResponseHeaders { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body of the HTTP response received from the webhook request.
    /// </summary>
    [Column(Name = "responseBody")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether an exception has occurred during webhook execution.
    /// </summary>
    [Column(Name = "exceptionOccured")]
    public bool ExceptionOccured { get; set; }
}
