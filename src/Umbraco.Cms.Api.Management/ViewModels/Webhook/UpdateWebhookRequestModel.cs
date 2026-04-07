namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

/// <summary>
/// Represents a request model for updating an existing webhook.
/// </summary>
public class UpdateWebhookRequestModel : WebhookModelBase
{
    /// <summary>
    /// Gets or sets the list of event names that will trigger the webhook.
    /// </summary>
    public string[] Events { get; set; } = Array.Empty<string>();
}
