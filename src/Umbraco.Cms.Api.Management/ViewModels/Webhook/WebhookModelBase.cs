using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

/// <summary>
/// Serves as the base class for webhook-related view models in the management API.
/// </summary>
public class WebhookModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether the webhook is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the webhook.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the webhook.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL for the webhook.
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the keys of the content types associated with the webhook.
    /// </summary>
    public Guid[] ContentTypeKeys { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Gets or sets the collection of HTTP headers associated with the webhook.
    /// </summary>
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
