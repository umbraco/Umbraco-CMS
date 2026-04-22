namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

/// <summary>
/// Represents the data for a webhook event, including event details and metadata, used for API responses.
/// </summary>
public class WebhookEventViewModel
{
    /// <summary>
    /// Gets or sets the name of the webhook event.
    /// </summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the webhook event.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique alias that identifies the webhook event.
    /// </summary>
    public string Alias { get; set; } = string.Empty;
}
