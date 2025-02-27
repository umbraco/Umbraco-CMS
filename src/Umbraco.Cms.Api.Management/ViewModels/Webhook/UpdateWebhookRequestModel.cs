namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

public class UpdateWebhookRequestModel : WebhookModelBase
{
    public string[] Events { get; set; } = Array.Empty<string>();
}
