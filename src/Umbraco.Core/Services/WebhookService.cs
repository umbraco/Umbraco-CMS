using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Services;

public class WebhookService : IWebHookService
{
    private readonly IWebhookRepository _webhookRepository;

    public WebhookService(IWebhookRepository webhookRepository) => _webhookRepository = webhookRepository;

    public Task CreateAsync(Webhook webhook)
    {
        _webhookRepository.Save(webhook);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Webhook webhook)
    {
        _webhookRepository.Delete(webhook);
        return Task.CompletedTask;
    }

    public Task<Webhook?> GetAsync(Guid key)
    {
        Webhook? webhook = _webhookRepository.Get(key);
        return Task.FromResult(webhook);
    }

    public Task<IEnumerable<Webhook>> GetMultipleAsync(IEnumerable<Guid> keys)
    {
        IEnumerable<Webhook> webhooks = _webhookRepository.GetMany(keys.ToArray());
        return Task.FromResult(webhooks);
    }
}
