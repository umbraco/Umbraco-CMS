using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class CacheService : ICacheService
{
    private readonly INuCacheContentRepository _nuCacheContentRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;

    public CacheService(
        INuCacheContentRepository nuCacheContentRepository,
        IIdKeyMap idKeyMap,
        ICoreScopeProvider scopeProvider)
    {
        _nuCacheContentRepository = nuCacheContentRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
    }

    public async Task<ContentCacheNode?> GetByKey(Guid key, bool preview = false)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document);
        if (idAttempt.Success is false)
        {
            return null;
        }

        ContentCacheNode contentCacheNode = _nuCacheContentRepository.GetContentSource(idAttempt.Result);
        scope.Complete();
        return await Task.FromResult(contentCacheNode);
    }

    public async Task<ContentCacheNode?> GetById(int id, bool preview = false)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        ContentCacheNode contentCacheNode = _nuCacheContentRepository.GetContentSource(id);
        scope.Complete();
        return await Task.FromResult(contentCacheNode);
    }

    public Task RefreshContent(IContent content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _nuCacheContentRepository.RefreshContent(content);
        scope.Complete();
        return Task.CompletedTask;
    }
}
