namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

/// <summary>
/// Represents a model used to return information about a webhook event in API responses.
/// </summary>
public class WebhookEventResponseModel
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
    /// Gets or sets the alias of the webhook event.
    /// </summary>
    public string Alias { get; set; } = string.Empty;
}
