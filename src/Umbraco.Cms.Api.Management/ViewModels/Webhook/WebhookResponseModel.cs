namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

/// <summary>
/// Represents a model containing data returned in response to a webhook request.
/// </summary>
public class WebhookResponseModel : WebhookModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the webhook response.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the collection of events associated with the webhook.
    /// </summary>
    public WebhookEventResponseModel[] Events { get; set; } = Array.Empty<WebhookEventResponseModel>();
}
