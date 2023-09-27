using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class WebhookService : IWebHookService
{
    private readonly ICoreScopeProvider _provider;
    private readonly IWebhookRepository _webhookRepository;

    public WebhookService(ICoreScopeProvider provider, IWebhookRepository webhookRepository)
    {
        _provider = provider;
        _webhookRepository = webhookRepository;
    }

    public async Task<Webhook> CreateAsync(Webhook webhook)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        Webhook created = await _webhookRepository.CreateAsync(webhook);
        scope.Complete();

        return created;
    }

    public async Task UpdateAsync(Webhook updateModel)
    {
        using ICoreScope scope = _provider.CreateCoreScope();

        // TODO: Validation if we need it

        Webhook? currentWebhook = await _webhookRepository.GetAsync(updateModel.Key);

        if (currentWebhook is null)
        {
            throw new ArgumentException("Webhook does not exist");
        }

        currentWebhook.Enabled = updateModel.Enabled;
        currentWebhook.EntityKeys = updateModel.EntityKeys;
        currentWebhook.Events = updateModel.Events;
        currentWebhook.Url = updateModel.Url;

        await _webhookRepository.UpdateAsync(currentWebhook);
        scope.Complete();
    }

    public async Task DeleteAsync(Guid key)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        Webhook? webhook = await _webhookRepository.GetAsync(key);
        if (webhook is not null)
        {
            await _webhookRepository.DeleteAsync(webhook);
        }

        scope.Complete();
    }

    public async Task<Webhook?> GetAsync(Guid key)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        Webhook? webhook = await _webhookRepository.GetAsync(key);
        scope.Complete();
        return webhook;
    }

    public async Task<PagedModel<Webhook>> GetAllAsync(int skip, int take)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        PagedModel<Webhook> webhooks = await _webhookRepository.GetAllAsync(skip, take);
        scope.Complete();

        return webhooks;
    }

    public Task<IEnumerable<Webhook>> GetByEventName(string eventName)
    {
        // using ICoreScope scope = ScopeProvider.CreateCoreScope();
        // IQuery<Webhook> query = Query<Webhook>().Where(x => x.Events.Contains(eventName));
        // IEnumerable<Webhook> webhooks = _webhookRepository.Get(query);
        // scope.Complete();
        // return Task.FromResult(webhooks);
        return Task.FromResult(Enumerable.Empty<Webhook>());

    }
}
