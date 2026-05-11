using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;

internal sealed class ContentTypeSeedKeyProvider : IDocumentSeedKeyProvider
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly IPublishStatusQueryService _publishStatusService;
    private readonly CacheSettings _cacheSettings;

    public ContentTypeSeedKeyProvider(
        ICoreScopeProvider scopeProvider,
        IDatabaseCacheRepository databaseCacheRepository,
        IOptions<CacheSettings> cacheSettings,
        IPublishStatusQueryService publishStatusService)
    {
        _scopeProvider = scopeProvider;
        _databaseCacheRepository = databaseCacheRepository;
        _publishStatusService = publishStatusService;
        _cacheSettings = cacheSettings.Value;
    }

    public ISet<Guid> GetSeedKeys()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        var documentKeys = _databaseCacheRepository
            .GetDocumentKeysByContentTypeKeys(_cacheSettings.ContentTypeKeys, published: true)
            .Where(key => _publishStatusService.IsDocumentPublishedInAnyCulture(key))
            .ToHashSet();
        scope.Complete();

        return documentKeys;
    }
}
