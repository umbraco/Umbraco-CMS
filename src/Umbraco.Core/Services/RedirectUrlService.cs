using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;

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
    public async Task RegisterAsync(string url, Guid contentKey, string? culture = null)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRedirectUrl? redir = await _redirectUrlRepository.GetAsync(url, contentKey, culture);
        if (redir != null)
        {
            redir.CreateDateUtc = DateTime.UtcNow;
        }
        else
        {
            redir = new RedirectUrl { Key = Guid.NewGuid(), Url = url, ContentKey = contentKey, Culture = culture };
        }

        await _redirectUrlRepository.SaveAsync(redir, CancellationToken.None);
        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(IRedirectUrl redirectUrl)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        await _redirectUrlRepository.DeleteAsync(redirectUrl, CancellationToken.None);
        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        await _redirectUrlRepository.DeleteAsync(id);
        scope.Complete();
    }

    /// <inheritdoc/>
    public async Task DeleteContentRedirectUrlsAsync(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        await _redirectUrlRepository.DeleteContentUrlsAsync(contentKey);
        scope.Complete();
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
        var recentUrl = await _redirectUrlRepository.GetMostRecentUrlAsync(url);
        scope.Complete();

        return recentUrl;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        var redirectUrl = await _redirectUrlRepository.GetContentUrlsAsync(contentKey);
        scope.Complete();

        return redirectUrl;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetContentRedirectUrlsAsync(Guid contentKey, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.GetContentUrlsAsync(contentKey, pageNumber, pageSize);

        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.GetAllUrlsAsync(pageNumber, pageSize);

        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> GetAllRedirectUrlsAsync(int rootContentId, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.GetAllUrlsAsync(rootContentId, pageNumber, pageSize);

        scope.Complete();
        return redirectUrls;
    }

    /// <inheritdoc/>
    public async Task<PagedModel<IRedirectUrl>> SearchRedirectUrlsAsync(string searchTerm, int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        PagedModel<IRedirectUrl> redirectUrls = await _redirectUrlRepository.SearchUrlsAsync(searchTerm, pageNumber, pageSize);

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
