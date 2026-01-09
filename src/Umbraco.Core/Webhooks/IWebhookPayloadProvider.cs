namespace Umbraco.Cms.Core.Webhooks;

public interface IWebhookPayloadProvider
{
    bool CanHandle(WebhookContext ctx);

    object BuildPayload(WebhookContext ctx);
}
