using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

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
    public void Register(string url, Guid contentKey, string? culture = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        IRedirectUrl? redir = _redirectUrlRepository.Get(url, contentKey, culture);
        if (redir != null)
        {
            redir.CreateDateUtc = DateTime.UtcNow;
        }
        else
        {
            redir = new RedirectUrl { Key = Guid.NewGuid(), Url = url, ContentKey = contentKey, Culture = culture };
        }

        _redirectUrlRepository.Save(redir);
        scope.Complete();
    }

    /// <inheritdoc/>
    public void Delete(IRedirectUrl redirectUrl)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        _redirectUrlRepository.Delete(redirectUrl);
        scope.Complete();
    }

    /// <inheritdoc/>
    public void Delete(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        _redirectUrlRepository.Delete(id);
        scope.Complete();
    }

    /// <inheritdoc/>
    public void DeleteContentRedirectUrls(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        _redirectUrlRepository.DeleteContentUrls(contentKey);
        scope.Complete();
    }

    /// <inheritdoc/>
    public void DeleteAll()
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        _redirectUrlRepository.DeleteAll();
        scope.Complete();
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
