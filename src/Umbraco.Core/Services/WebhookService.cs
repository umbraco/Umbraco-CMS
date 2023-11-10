using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class WebhookService : IWebhookService
{
    private readonly ICoreScopeProvider _provider;
    private readonly IWebhookRepository _webhookRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;

    public WebhookService(ICoreScopeProvider provider, IWebhookRepository webhookRepository, IEventMessagesFactory eventMessagesFactory)
    {
        _provider = provider;
        _webhookRepository = webhookRepository;
        _eventMessagesFactory = eventMessagesFactory;
    }

    public async Task<Webhook?> CreateAsync(Webhook webhook)
    {
        using ICoreScope scope = _provider.CreateCoreScope();

        EventMessages eventMessages = _eventMessagesFactory.Get();
        var savingNotification = new WebhookSavingNotification(webhook, eventMessages);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return null;
        }

        Webhook created = await _webhookRepository.CreateAsync(webhook);

        scope.Notifications.Publish(
            new WebhookSavedNotification(webhook, eventMessages).WithStateFrom(savingNotification));

        scope.Complete();

        return created;
    }

    public async Task UpdateAsync(Webhook webhook)
    {
        using ICoreScope scope = _provider.CreateCoreScope();

        Webhook? currentWebhook = await _webhookRepository.GetAsync(webhook.Key);

        if (currentWebhook is null)
        {
            throw new ArgumentException("Webhook does not exist");
        }

        EventMessages eventMessages = _eventMessagesFactory.Get();
        var savingNotification = new WebhookSavingNotification(webhook, eventMessages);
        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return;
        }

        currentWebhook.Enabled = webhook.Enabled;
        currentWebhook.ContentTypeKeys = webhook.ContentTypeKeys;
        currentWebhook.Events = webhook.Events;
        currentWebhook.Url = webhook.Url;
        currentWebhook.Headers = webhook.Headers;

        await _webhookRepository.UpdateAsync(currentWebhook);

        scope.Notifications.Publish(
            new WebhookSavedNotification(webhook, eventMessages).WithStateFrom(savingNotification));

        scope.Complete();
    }

    public async Task DeleteAsync(Guid key)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        Webhook? webhook = await _webhookRepository.GetAsync(key);
        if (webhook is not null)
        {
            EventMessages eventMessages = _eventMessagesFactory.Get();
            var deletingNotification = new WebhookDeletingNotification(webhook, eventMessages);
            if (scope.Notifications.PublishCancelable(deletingNotification))
            {
                scope.Complete();
                return;
            }

            await _webhookRepository.DeleteAsync(webhook);
            scope.Notifications.Publish(
                new WebhookDeletedNotification(webhook, eventMessages).WithStateFrom(deletingNotification));
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

    public async Task<IEnumerable<Webhook>> GetByEventNameAsync(string eventName)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        PagedModel<Webhook> webhooks = await _webhookRepository.GetByEventNameAsync(eventName);
        scope.Complete();

        return webhooks.Items;
    }
}
