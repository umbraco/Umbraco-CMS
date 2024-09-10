using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders;

internal class ContentTypeSeedKeyProvider : IDocumentSeedKeyProvider
{
    private Guid[]? _documentKeys = null;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly CacheSettings _cacheSettings;

    public ContentTypeSeedKeyProvider(
        ICoreScopeProvider scopeProvider,
        IDatabaseCacheRepository databaseCacheRepository,
        IOptions<CacheSettings> cacheSettings)
    {
        _scopeProvider = scopeProvider;
        _databaseCacheRepository = databaseCacheRepository;
        _cacheSettings = cacheSettings.Value;
    }

    public IEnumerable<Guid> GetSeedKeys()
    {
        if (_documentKeys is not null)
        {
            return _documentKeys;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _documentKeys = _databaseCacheRepository.GetContentKeysByContentTypeKeys(_cacheSettings.ContentTypeKeys).ToArray();
        scope.Complete();

        return _documentKeys;
    }
}
