namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a log entry for a webhook request execution.
/// </summary>
public class WebhookLog
{
    /// <summary>
    ///     Gets or sets the unique identifier of the log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the unique key of the webhook that was executed.
    /// </summary>
    public Guid WebhookKey { get; set; }

    /// <summary>
    ///     Gets or sets the unique key of this log entry.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the URL that the webhook request was sent to.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the HTTP status code returned from the webhook request.
    /// </summary>
    public string StatusCode { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the webhook request was executed.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the event that triggered the webhook.
    /// </summary>
    public string EventAlias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the number of retry attempts for this webhook request.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    ///     Gets or sets the HTTP headers that were sent with the request.
    /// </summary>
    public string RequestHeaders { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the body content that was sent with the request.
    /// </summary>
    public string? RequestBody { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the HTTP headers returned in the response.
    /// </summary>
    public string ResponseHeaders { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the body content returned in the response.
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether an exception occurred during the request.
    /// </summary>
    public bool ExceptionOccured { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the response had a success status code (2xx).
    /// </summary>
    public bool IsSuccessStatusCode { get; set; }
}
