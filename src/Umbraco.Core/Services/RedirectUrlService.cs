using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing redirect URLs.
/// </summary>
internal sealed class RedirectUrlService : AsyncRepositoryService, IRedirectUrlService
{
    private readonly IRedirectUrlRepository _redirectUrlRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUrlService"/> class.
    /// </summary>
    public RedirectUrlService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IRedirectUrlRepository redirectUrlRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _redirectUrlRepository = redirectUrlRepository;

    /// <inheritdoc/>
    [Obsolete("Use RegisterWithStatusAsync instead. Scheduled for removal in Umbraco 20.")]
    public void Register(string url, Guid contentKey, string? culture = null)
        => RegisterWithStatusAsync(url, contentKey, culture).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task<Attempt<IRedirectUrl?, RedirectUrlOperationStatus>> RegisterWithStatusAsync(string oldUrl, Guid contentKey, string? culture = null)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRedirectUrl? redir = await _redirectUrlRepository.GetAsync(oldUrl, contentKey, culture);
        if (redir != null)
        {
            redir.CreateDateUtc = DateTime.UtcNow;
        }
        else
        {
            redir = new RedirectUrl { Key = Guid.NewGuid(), Url = oldUrl, ContentKey = contentKey, Culture = culture };
        }

        // Use a detached EventMessages instance so a handler cancelling the save does not surface a
        // notification in the backoffice. Redirect creation is a silent side-effect of publishing, so a
        // cancellation is not something the editor triggered or can act on - unlike deletion (an explicit
        // editor action), where the sibling methods deliberately use EventMessagesFactory.Get() instead.
        var eventMessages = new EventMessages();
        var savingNotification = new RedirectUrlSavingNotification(redir, eventMessages);

        if (scope.Notifications.PublishCancelable(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IRedirectUrl?, RedirectUrlOperationStatus>(RedirectUrlOperationStatus.CancelledByNotification, redir);
        }

        await _redirectUrlRepository.SaveAsync(redir, CancellationToken.None);

        scope.Notifications.Publish(new RedirectUrlSavedNotification(redir, eventMessages)
                .WithStateFrom(savingNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus<IRedirectUrl?, RedirectUrlOperationStatus>(RedirectUrlOperationStatus.Success, redir);
    }

    /// <inheritdoc/>
    [Obsolete("Use DeleteWithStatusAsync(IRedirectUrl) instead. Scheduled for removal in Umbraco 20.")]
    public void Delete(IRedirectUrl redirectUrl) => DeleteWithStatusAsync(redirectUrl).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task<RedirectUrlOperationStatus> DeleteWithStatusAsync(IRedirectUrl redirectUrl)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        EventMessages eventMessages = EventMessagesFactory.Get();

        var deletingNotification = new RedirectUrlDeletingNotification(redirectUrl, eventMessages);

        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            scope.Complete();
            return RedirectUrlOperationStatus.CancelledByNotification;
        }

        await _redirectUrlRepository.DeleteAsync(redirectUrl, CancellationToken.None);

        scope.Notifications.Publish(new RedirectUrlDeletedNotification(redirectUrl, eventMessages)
            .WithStateFrom(deletingNotification));

        scope.Complete();
        return RedirectUrlOperationStatus.Success;
    }

    /// <inheritdoc/>
    [Obsolete("Use DeleteWithStatusAsync(Guid) instead. Scheduled for removal in Umbraco 20.")]
    public void Delete(Guid id) => DeleteWithStatusAsync(id).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task<RedirectUrlOperationStatus> DeleteWithStatusAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        IRedirectUrl? redirectUrl = await _redirectUrlRepository.GetAsync(id, CancellationToken.None);

        if (redirectUrl is null)
        {
            scope.Complete();
            return RedirectUrlOperationStatus.NotFound;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var deletingNotification = new RedirectUrlDeletingNotification(redirectUrl, eventMessages);

        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            scope.Complete();
            return RedirectUrlOperationStatus.CancelledByNotification;
        }

        await _redirectUrlRepository.DeleteAsync(redirectUrl, CancellationToken.None);

        scope.Notifications.Publish(new RedirectUrlDeletedNotification(redirectUrl, eventMessages)
            .WithStateFrom(deletingNotification));

        scope.Complete();
        return RedirectUrlOperationStatus.Success;
    }

    /// <inheritdoc/>
    [Obsolete("Use DeleteContentRedirectUrlsWithStatusAsync instead. Scheduled for removal in Umbraco 20.")]
    public void DeleteContentRedirectUrls(Guid contentKey) => DeleteContentRedirectUrlsWithStatusAsync(contentKey).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task<RedirectUrlOperationStatus> DeleteContentRedirectUrlsWithStatusAsync(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        IRedirectUrl[] redirectUrls = (await _redirectUrlRepository.GetContentUrlsAsync(contentKey)).ToArray();

        if (redirectUrls.Length == 0)
        {
            scope.Complete();
            return RedirectUrlOperationStatus.Success;
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var deletingNotification = new RedirectUrlDeletingNotification(redirectUrls, eventMessages);

        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            scope.Complete();
            return RedirectUrlOperationStatus.CancelledByNotification;
        }

        await _redirectUrlRepository.DeleteContentUrlsAsync(contentKey);

        scope.Notifications.Publish(new RedirectUrlDeletedNotification(redirectUrls, eventMessages)
            .WithStateFrom(deletingNotification));

        scope.Complete();
        return RedirectUrlOperationStatus.Success;
    }

    /// <inheritdoc/>
    public async Task DeleteAllAsync()
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        await _redirectUrlRepository.DeleteAllAsync();
        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRedirectUrl? redirectUrl = await _redirectUrlRepository.GetMostRecentUrlAsync(url);
        scope.Complete();
        return redirectUrl;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IRedirectUrl> redirectUrls = await _redirectUrlRepository.GetContentUrlsAsync(contentKey);
        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.GetContentUrlsAsync(contentKey, skip, take);
        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.GetAllUrlsAsync(skip, take);
        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int rootContentId, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.GetAllUrlsAsync(rootContentId, skip, take);
        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> SearchRedirectUrlsAsync(string searchTerm, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.SearchUrlsAsync(searchTerm, skip, take);
        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url, string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return await GetMostRecentRedirectUrlAsync(url);
        }

        using ICoreScope scope = ScopeProvider.CreateScope();
        IRedirectUrl? redirectUrl = await _redirectUrlRepository.GetMostRecentUrlAsync(url, culture);
        scope.Complete();
        return redirectUrl;
    }
}
