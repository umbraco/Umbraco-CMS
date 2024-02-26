namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

public class WebhookResponseModel : WebhookModelBase
{
    public Guid Id { get; set; }

    public WebhookEventResponseModel[] Events { get; set; } = Array.Empty<WebhookEventResponseModel>();
}
