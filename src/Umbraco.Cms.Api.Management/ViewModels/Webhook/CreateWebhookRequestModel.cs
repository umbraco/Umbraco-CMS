namespace Umbraco.Cms.Api.Management.ViewModels.Webhook;

public class CreateWebhookRequestModel : WebhookModelBase
{
    public Guid? Id { get; set; }

    public string[] Events { get; set; } = Array.Empty<string>();
}
