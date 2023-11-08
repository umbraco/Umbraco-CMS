using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class WebhookRequestRepository : IWebhookRequestRepository
{
    public Task<WebhookRequest> CreateAsync(WebhookRequest webhookRequest)
    {
        return Task.CompletedTask;
    }
}
