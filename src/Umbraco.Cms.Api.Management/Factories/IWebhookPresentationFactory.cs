using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Logs;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IWebhookPresentationFactory
{
    WebhookResponseModel CreateResponseModel(IWebhook webhook);

    IWebhook CreateWebhook(CreateWebhookRequestModel webhookRequestModel);

    IWebhook CreateWebhook(UpdateWebhookRequestModel webhookRequestModel, Guid existingWebhookKey);

    WebhookLogResponseModel CreateResponseModel(WebhookLog webhookLog) => new();
}
