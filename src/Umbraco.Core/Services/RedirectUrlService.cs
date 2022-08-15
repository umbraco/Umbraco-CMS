using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

internal class RedirectUrlService : RepositoryService, IRedirectUrlService
{
    private readonly IRedirectUrlRepository _redirectUrlRepository;

    public RedirectUrlService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IRedirectUrlRepository redirectUrlRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _redirectUrlRepository = redirectUrlRepository;

    public void Register(string url, Guid contentKey, string? culture = null)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
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
    }

    public void Delete(IRedirectUrl redirectUrl)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _redirectUrlRepository.Delete(redirectUrl);
            scope.Complete();
        }
    }

    public void Delete(Guid id)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _redirectUrlRepository.Delete(id);
            scope.Complete();
        }
    }

    public void DeleteContentRedirectUrls(Guid contentKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _redirectUrlRepository.DeleteContentUrls(contentKey);
            scope.Complete();
        }
    }

    public void DeleteAll()
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _redirectUrlRepository.DeleteAll();
            scope.Complete();
        }
    }

    public IRedirectUrl? GetMostRecentRedirectUrl(string url)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _redirectUrlRepository.GetMostRecentUrl(url);
        }
    }

    public async Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url)
    {
        using (var scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await _redirectUrlRepository.GetMostRecentUrlAsync(url);
        }
    }

    public IEnumerable<IRedirectUrl> GetContentRedirectUrls(Guid contentKey)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _redirectUrlRepository.GetContentUrls(contentKey);
        }
    }

    public IEnumerable<IRedirectUrl> GetAllRedirectUrls(long pageIndex, int pageSize, out long total)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _redirectUrlRepository.GetAllUrls(pageIndex, pageSize, out total);
        }
    }

    public IEnumerable<IRedirectUrl> GetAllRedirectUrls(int rootContentId, long pageIndex, int pageSize, out long total)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _redirectUrlRepository.GetAllUrls(rootContentId, pageIndex, pageSize, out total);
        }
    }

    public IEnumerable<IRedirectUrl> SearchRedirectUrls(string searchTerm, long pageIndex, int pageSize, out long total)
    {
        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _redirectUrlRepository.SearchUrls(searchTerm, pageIndex, pageSize, out total);
        }
    }

    public IRedirectUrl? GetMostRecentRedirectUrl(string url, string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return GetMostRecentRedirectUrl(url);
        }

        using (ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return _redirectUrlRepository.GetMostRecentUrl(url, culture);
        }
    }

    public async Task<IRedirectUrl?> GetMostRecentRedirectUrlAsync(string url, string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return await GetMostRecentRedirectUrlAsync(url);
        }

        using (var scope = ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            return await _redirectUrlRepository.GetMostRecentUrlAsync(url, culture);
        }
    }	
}
