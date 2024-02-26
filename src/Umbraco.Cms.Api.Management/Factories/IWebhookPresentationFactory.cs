using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IWebhookPresentationFactory
{
    WebhookResponseModel CreateResponseModel(IWebhook webhook);

    IWebhook CreateWebhook(CreateWebhookRequestModel webhookRequestModel);

    IWebhook CreateWebhook(UpdateWebhookRequestModel webhookRequestModel);
}
