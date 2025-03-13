using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

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

    private WebhookOperationStatus ValidateWebhook(IWebhook webhook)
    {
        if (webhook.Events.Length <= 0)
        {
            return WebhookOperationStatus.NoEvents;
        }

        return WebhookOperationStatus.Success;
    }

    /// <inheritdoc />
    public async Task<Attempt<IWebhook, WebhookOperationStatus>> CreateAsync(IWebhook webhook)
    {
        WebhookOperationStatus validationResult = ValidateWebhook(webhook);
        if (validationResult is not WebhookOperationStatus.Success)
        {
            return Attempt.FailWithStatus(validationResult, webhook);
        }

        using ICoreScope scope = _provider.CreateCoreScope();

        EventMessages eventMessages = _eventMessagesFactory.Get();
        var savingNotification = new WebhookSavingNotification(webhook, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(WebhookOperationStatus.CancelledByNotification, webhook);
        }

        IWebhook created = await _webhookRepository.CreateAsync(webhook);

        scope.Notifications.Publish(new WebhookSavedNotification(webhook, eventMessages).WithStateFrom(savingNotification));

        scope.Complete();

        return Attempt.SucceedWithStatus(WebhookOperationStatus.Success, created);
    }

    /// <inheritdoc />
    public async Task<Attempt<IWebhook, WebhookOperationStatus>> UpdateAsync(IWebhook webhook)
    {
        WebhookOperationStatus validationResult = ValidateWebhook(webhook);
        if (validationResult is not WebhookOperationStatus.Success)
        {
            return Attempt.FailWithStatus(validationResult, webhook);
        }

        using ICoreScope scope = _provider.CreateCoreScope();

        IWebhook? currentWebhook = await _webhookRepository.GetAsync(webhook.Key);

        if (currentWebhook is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus(WebhookOperationStatus.NotFound, webhook);
        }

        EventMessages eventMessages = _eventMessagesFactory.Get();
        var savingNotification = new WebhookSavingNotification(webhook, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(WebhookOperationStatus.CancelledByNotification, webhook);
        }

        currentWebhook.Enabled = webhook.Enabled;
        currentWebhook.ContentTypeKeys = webhook.ContentTypeKeys;
        currentWebhook.Events = webhook.Events;
        currentWebhook.Url = webhook.Url;
        currentWebhook.Headers = webhook.Headers;

        await _webhookRepository.UpdateAsync(currentWebhook);

        scope.Notifications.Publish(new WebhookSavedNotification(webhook, eventMessages).WithStateFrom(savingNotification));

        scope.Complete();

        return Attempt.SucceedWithStatus(WebhookOperationStatus.Success, webhook);
    }

    /// <inheritdoc />
    public async Task<Attempt<IWebhook?, WebhookOperationStatus>> DeleteAsync(Guid key)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        IWebhook? webhook = await _webhookRepository.GetAsync(key);
        if (webhook is null)
        {
            return Attempt.FailWithStatus(WebhookOperationStatus.NotFound, webhook);
        }

        EventMessages eventMessages = _eventMessagesFactory.Get();
        var deletingNotification = new WebhookDeletingNotification(webhook, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IWebhook?, WebhookOperationStatus>(WebhookOperationStatus.CancelledByNotification, webhook);
        }

        await _webhookRepository.DeleteAsync(webhook);
        scope.Notifications.Publish(new WebhookDeletedNotification(webhook, eventMessages).WithStateFrom(deletingNotification));

        scope.Complete();

        return Attempt.SucceedWithStatus<IWebhook?, WebhookOperationStatus>(WebhookOperationStatus.Success, webhook);
    }

    /// <inheritdoc />
    public async Task<IWebhook?> GetAsync(Guid key)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        IWebhook? webhook = await _webhookRepository.GetAsync(key);
        scope.Complete();
        return webhook;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IWebhook>> GetAllAsync(int skip, int take)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        PagedModel<IWebhook> webhooks = await _webhookRepository.GetAllAsync(skip, take);
        scope.Complete();

        return webhooks;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IWebhook>> GetByAliasAsync(string alias)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        PagedModel<IWebhook> webhooks = await _webhookRepository.GetByAliasAsync(alias);
        scope.Complete();

        return webhooks.Items;
    }
}
