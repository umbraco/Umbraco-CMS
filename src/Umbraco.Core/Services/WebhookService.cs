using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class WebhookService : IWebHookService
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public WebhookService(IWebhookRepository webhookRepository, ICoreScopeProvider coreScopeProvider)
    {
        _webhookRepository = webhookRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public Task<Webhook> CreateAsync(Webhook webhook)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        _webhookRepository.Save(webhook);
        scope.Complete();

        return Task.FromResult(webhook);
    }

    public Task UpdateAsync(Webhook updateModel)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        // TODO: Validation if we need it

        Webhook? currentWebhook = _webhookRepository.Get(updateModel.Key);

        if (currentWebhook is null)
        {
            throw new ArgumentException("Webhook does not exist");
        }

        currentWebhook.Enabled = updateModel.Enabled;
        currentWebhook.EntityKeys = updateModel.EntityKeys;
        currentWebhook.Event = updateModel.Event;
        currentWebhook.Url = updateModel.Url;

        _webhookRepository.Save(currentWebhook);
        scope.Complete();

        return Task.FromResult(updateModel);
    }

    public Task DeleteAsync(Guid key)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        Webhook? webhook = _webhookRepository.Get(key);
        if (webhook is not null)
        {
            _webhookRepository.Delete(webhook);
        }

        scope.Complete();
        return Task.CompletedTask;
    }

    public Task<Webhook?> GetAsync(Guid key)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        Webhook? webhook = _webhookRepository.Get(key);
        scope.Complete();
        return Task.FromResult(webhook);
    }

    public Task<IEnumerable<Webhook>> GetMultipleAsync(IEnumerable<Guid> keys)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IEnumerable<Webhook> webhooks = _webhookRepository.GetMany(keys.ToArray());
        scope.Complete();
        return Task.FromResult(webhooks);
    }

    public Task<IEnumerable<Webhook>> GetAllAsync()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        IEnumerable<Webhook> webhooks = _webhookRepository.GetMany();
        scope.Complete();
        return Task.FromResult(webhooks);
    }
}
