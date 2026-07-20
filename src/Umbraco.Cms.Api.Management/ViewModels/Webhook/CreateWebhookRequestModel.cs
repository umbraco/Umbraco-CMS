namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

/// <summary>
/// Represents a request model for creating a new webhook, containing the necessary data for webhook registration.
/// </summary>
public class CreateWebhookRequestModel : WebhookModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the webhook.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the list of event names that will trigger the webhook.
    /// </summary>
    public string[] Events { get; set; } = Array.Empty<string>();
}
