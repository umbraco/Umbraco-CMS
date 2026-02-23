namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a pending webhook request that needs to be sent.
/// </summary>
public class WebhookRequest
{
    /// <summary>
    ///     Gets or sets the unique identifier of the request.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the unique key of the webhook this request belongs to.
    /// </summary>
    public Guid WebhookKey { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the event that triggered this webhook request.
    /// </summary>
    public string EventAlias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the serialized request payload to send.
    /// </summary>
    public string? RequestObject { get; set; }

    /// <summary>
    ///     Gets or sets the number of retry attempts made for this request.
    /// </summary>
    public int RetryCount { get; set; }
}
