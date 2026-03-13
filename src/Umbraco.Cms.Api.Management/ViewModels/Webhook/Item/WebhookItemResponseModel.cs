using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Webhook.Item;

/// <summary>
/// Represents a webhook item returned in an API response.
/// </summary>
public class WebhookItemResponseModel : ItemResponseModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether the webhook is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the webhook.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a comma-separated list of events that trigger the webhook.
    /// </summary>
    public string Events { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the webhook.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the types associated with the webhook item, typically as a comma-separated list of type names or identifiers.
    /// </summary>
    public string Types { get; set; } = string.Empty;
}
