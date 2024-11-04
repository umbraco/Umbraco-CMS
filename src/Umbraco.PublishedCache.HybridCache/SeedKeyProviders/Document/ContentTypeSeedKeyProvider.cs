using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;

internal sealed class ContentTypeSeedKeyProvider : IDocumentSeedKeyProvider
{
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

    public ISet<Guid> GetSeedKeys()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        var documentKeys = _databaseCacheRepository.GetDocumentKeysByContentTypeKeys(_cacheSettings.ContentTypeKeys, published: true).ToHashSet();
        scope.Complete();

        return documentKeys;
    }
}
