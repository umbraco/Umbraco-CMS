using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides services for managing redirect URLs.
/// </summary>
internal sealed class RedirectUrlService : RepositoryService, IRedirectUrlService
{
    private readonly IRedirectUrlRepository _redirectUrlRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUrlService"/> class.
    /// </summary>
    public RedirectUrlService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IRedirectUrlRepository redirectUrlRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _redirectUrlRepository = redirectUrlRepository;

    /// <inheritdoc/>
    [Obsolete("Use the Register overload that takes newUrl. Scheduled for removal in Umbraco 20.")]
    public void Register(string url, Guid contentKey, string? culture = null)
        => Register(url, newUrl: null, contentKey, culture);

    /// <inheritdoc/>
    public Attempt<IRedirectUrl?, RedirectUrlOperationStatus> Register(string oldUrl, string? newUrl, Guid contentKey, string? culture = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IRedirectUrl? redir = _redirectUrlRepository.Get(oldUrl, contentKey, culture);
        if (redir != null)
        {
            redir.CreateDateUtc = DateTime.UtcNow;
        }
        else
        {
            redir = new RedirectUrl { Key = Guid.NewGuid(), Url = oldUrl, ContentKey = contentKey, Culture = culture };
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var creatingNotification = new RedirectUrlCreatingNotification(redir, newUrl, eventMessages);

        if (scope.Notifications.PublishCancelable(creatingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IRedirectUrl?, RedirectUrlOperationStatus>(RedirectUrlOperationStatus.CancelledByNotification, redir);
        }

        _redirectUrlRepository.Save(redir);

        scope.Notifications.Publish(new RedirectUrlCreatedNotification(redir, newUrl, eventMessages)
                .WithStateFrom(creatingNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus<IRedirectUrl?, RedirectUrlOperationStatus>(RedirectUrlOperationStatus.Success, redir);
    }

    /// <inheritdoc/>
    [Obsolete("Use DeleteWithStatus(IRedirectUrl) instead. Scheduled for removal in Umbraco 20.")]
    public void Delete(IRedirectUrl redirectUrl) => DeleteWithStatus(redirectUrl);

    /// <inheritdoc/>
    public RedirectUrlOperationStatus DeleteWithStatus(IRedirectUrl redirectUrl)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        EventMessages eventMessages = EventMessagesFactory.Get();

        var deletingNotification = new RedirectUrlDeletingNotification(redirectUrl, eventMessages);

        if (scope.Notifications.PublishCancelable(deletingNotification))
        {
            scope.Complete();
            return RedirectUrlOperationStatus.CancelledByNotification;
        }

        _redirectUrlRepository.Delete(redirectUrl);

        scope.Notifications.Publish(new RedirectUrlDeletedNotification(redirectUrl, eventMessages)
            .WithStateFrom(deletingNotification));

        scope.Complete();
        return RedirectUrlOperationStatus.Success;
    }

    /// <inheritdoc/>
    [Obsolete("Use DeleteWithStatus(Guid) instead. Scheduled for removal in Umbraco 20.")]
    public void Delete(Guid id) => DeleteWithStatus(id);

    /// <inheritdoc/>
    public RedirectUrlOperationStatus DeleteWithStatus(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IRedirectUrl? redirectUrl = _redirectUrlRepository.Get(id);

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

        _redirectUrlRepository.Delete(id);

        scope.Notifications.Publish(new RedirectUrlDeletedNotification(redirectUrl, eventMessages)
            .WithStateFrom(deletingNotification));

        scope.Complete();
        return RedirectUrlOperationStatus.Success;
    }

    /// <inheritdoc/>
    [Obsolete("Use DeleteContentRedirectUrlsWithStatus instead. Scheduled for removal in Umbraco 20.")]
    public void DeleteContentRedirectUrls(Guid contentKey) => DeleteContentRedirectUrlsWithStatus(contentKey);

    /// <inheritdoc/>
    public RedirectUrlOperationStatus DeleteContentRedirectUrlsWithStatus(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IRedirectUrl[] redirectUrls = _redirectUrlRepository.GetContentUrls(contentKey).ToArray();

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

        _redirectUrlRepository.DeleteContentUrls(contentKey);

        scope.Notifications.Publish(new RedirectUrlDeletedNotification(redirectUrls, eventMessages)
            .WithStateFrom(deletingNotification));

        scope.Complete();
        return RedirectUrlOperationStatus.Success;
    }

    /// <inheritdoc/>
    [Obsolete("Use DeleteAllWithStatus instead. Scheduled for removal in Umbraco 20.")]
    public void DeleteAll() => DeleteAllWithStatus();

    /// <inheritdoc/>
    public RedirectUrlOperationStatus DeleteAllWithStatus()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        // Pull the entire set so handlers can inspect what is about to be deleted. Redirect URL tables
        // are typically small, but if performance becomes an issue this could be paginated.
        IRedirectUrl[] redirectUrls = _redirectUrlRepository.GetAllUrls(0, int.MaxValue, out _).ToArray();

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

        _redirectUrlRepository.DeleteAll();

        scope.Notifications.Publish(new RedirectUrlDeletedNotification(redirectUrls, eventMessages).WithStateFrom(deletingNotification));

        scope.Complete();
        return RedirectUrlOperationStatus.Success;
    }

    /// <inheritdoc/>
    public IRedirectUrl? GetMostRecentRedirectUrl(string url)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return _redirectUrlRepository.GetMostRecentUrl(url);
    }

    /// <inheritdoc/>
    public async Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await _redirectUrlRepository.GetMostRecentUrlAsync(url);
    }

    /// <inheritdoc/>
    public IEnumerable<IRedirectUrl> GetContentRedirectUrls(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return _redirectUrlRepository.GetContentUrls(contentKey);
    }

    /// <inheritdoc/>
    public IEnumerable<IRedirectUrl> GetAllRedirectUrls(long pageIndex, int pageSize, out long total)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return _redirectUrlRepository.GetAllUrls(pageIndex, pageSize, out total);
    }

    /// <inheritdoc/>
    public IEnumerable<IRedirectUrl> GetAllRedirectUrls(int rootContentId, long pageIndex, int pageSize, out long total)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return _redirectUrlRepository.GetAllUrls(rootContentId, pageIndex, pageSize, out total);
    }

    /// <inheritdoc/>
    public IEnumerable<IRedirectUrl> SearchRedirectUrls(string searchTerm, long pageIndex, int pageSize, out long total)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return _redirectUrlRepository.SearchUrls(searchTerm, pageIndex, pageSize, out total);
    }

    /// <inheritdoc/>
    public IRedirectUrl? GetMostRecentRedirectUrl(string url, string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return GetMostRecentRedirectUrl(url);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return _redirectUrlRepository.GetMostRecentUrl(url, culture);
    }

    /// <inheritdoc/>
    public async Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url, string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return await GetMostRecentRedirectUrlAsync(url);
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return await _redirectUrlRepository.GetMostRecentUrlAsync(url, culture);
    }
}
