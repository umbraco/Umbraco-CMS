using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookLogService
{
    Task<WebhookLog> CreateAsync(WebhookLog webhookLog);

    Task<PagedModel<WebhookLog>> Get(int skip = 0, int take = int.MaxValue);
}
