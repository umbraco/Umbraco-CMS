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

    /// <inheritdoc />
    public async Task<Webhook> CreateAsync(Webhook webhook)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        Webhook created = await _webhookRepository.CreateAsync(webhook);
        scope.Complete();

        return created;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Webhook webhook)
    {
        using ICoreScope scope = _provider.CreateCoreScope();

        Webhook? currentWebhook = await _webhookRepository.GetAsync(webhook.Key);

        if (currentWebhook is null)
        {
            throw new ArgumentException("Webhook does not exist");
        }

        currentWebhook.Enabled = webhook.Enabled;
        currentWebhook.ContentTypeKeys = webhook.ContentTypeKeys;
        currentWebhook.Events = webhook.Events;
        currentWebhook.Url = webhook.Url;
        currentWebhook.Headers = webhook.Headers;

        await _webhookRepository.UpdateAsync(currentWebhook);
        scope.Complete();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Webhook?> GetAsync(Guid key)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        Webhook? webhook = await _webhookRepository.GetAsync(key);
        scope.Complete();
        return webhook;
    }

    /// <inheritdoc />
    public async Task<PagedModel<Webhook>> GetAllAsync(int skip, int take)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        PagedModel<Webhook> webhooks = await _webhookRepository.GetAllAsync(skip, take);
        scope.Complete();

        return webhooks;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Webhook>> GetByEventNameAsync(string eventName)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        PagedModel<Webhook> webhooks = await _webhookRepository.GetByEventNameAsync(eventName);
        scope.Complete();

        return webhooks.Items;
    }
}
