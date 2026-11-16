using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IWebhookService" /> to manage webhooks.
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly ICoreScopeProvider _provider;
    private readonly IWebhookRepository _webhookRepository;
    private readonly IEventMessagesFactory _eventMessagesFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebhookService" /> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="webhookRepository">The webhook repository.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    public WebhookService(ICoreScopeProvider provider, IWebhookRepository webhookRepository, IEventMessagesFactory eventMessagesFactory)
    {
        _provider = provider;
        _webhookRepository = webhookRepository;
        _eventMessagesFactory = eventMessagesFactory;
    }

    /// <summary>
    ///     Validates that a webhook has the required configuration.
    /// </summary>
    /// <param name="webhook">The webhook to validate.</param>
    /// <returns>The <see cref="WebhookOperationStatus" /> indicating the validation result.</returns>
    private WebhookOperationStatus ValidateWebhook(IWebhook webhook)
    {
        if (webhook.Events.Length <= 0)
        {
            return WebhookOperationStatus.NoEvents;
        }

        if (webhook.Headers.Any(header => IsValidHeader(header.Key, header.Value) is false))
        {
            return WebhookOperationStatus.InvalidHeaders;
        }

        return WebhookOperationStatus.Success;
    }

    // Values arrive from deserialized JSON, where a dictionary value can be null despite the annotation.
    private static bool IsValidHeader(string name, string? value)
        => IsHttpToken(name)
           && Constants.WebhookEvents.ReservedHeaderNames.Contains(name) is false
           && value is not null
           && IsSendableHeaderValue(value);

    // RFC 9110 section 5.6.2: a field name is a token of visible characters excluding delimiters.
    private static bool IsHttpToken(string value)
        => value.Length > 0 && value.All(c => char.IsAsciiLetterOrDigit(c) || "!#$%&'*+-.^_`|~".Contains(c));

    // HttpClient only sends ASCII request header values unless a header encoding is configured, so the value is
    // limited to visible ASCII, space and horizontal tab (which also excludes CR, LF and NUL).
    private static bool IsSendableHeaderValue(string value)
        => value.All(c => c == '\t' || (c >= ' ' && c <= '~'));

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
        currentWebhook.Name = webhook.Name;
        currentWebhook.Description = webhook.Description;
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

        scope.WriteLock(Constants.Locks.WebhookRequest);
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
    public async Task<IEnumerable<IWebhook?>> GetMultipleAsync(IEnumerable<Guid> keys)
    {
        using ICoreScope scope = _provider.CreateCoreScope();
        PagedModel<IWebhook> webhooks = await _webhookRepository.GetByIdsAsync(keys);
        scope.Complete();

        return webhooks.Items;
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
