namespace Umbraco.Cms.Api.Management.ViewModels.Webhook.Logs;

/// <summary>
/// Represents a model containing information about a single webhook log entry returned in the API response.
/// </summary>
public class WebhookLogResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the webhook log entry.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the webhook.
    /// </summary>
    public Guid WebhookKey { get; set; }

    /// <summary>
    /// Gets or sets the status code of the webhook response.
    /// </summary>
    public string StatusCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the HTTP response status code indicates success.
    /// </summary>
    public bool IsSuccessStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the webhook log entry was created.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the alias of the event associated with the webhook log.
    /// </summary>
    public string EventAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL associated with the webhook log entry.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of retry attempts made for the webhook.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the raw request headers that were sent with the webhook request and recorded in the log.
    /// </summary>
    public string RequestHeaders { get; set; } = string.Empty;

    /// <summary>Gets or sets the request body of the webhook log.</summary>
    public string RequestBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response headers returned by the webhook request, serialized as a string.
    /// The format of the string depends on the implementation (e.g., JSON or raw header format).
    /// </summary>
    public string ResponseHeaders { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response body returned by the webhook.
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether an exception occurred during the webhook execution.
    /// </summary>
    public bool ExceptionOccured { get; set; }
}
