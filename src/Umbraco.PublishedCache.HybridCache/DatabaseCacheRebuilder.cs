using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class DatabaseCacheRebuilder : IDatabaseCacheRebuilder
{
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public DatabaseCacheRebuilder(IDatabaseCacheRepository databaseCacheRepository, ICoreScopeProvider coreScopeProvider)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _coreScopeProvider = coreScopeProvider;
    }

    public void Rebuild()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild();
        scope.Complete();
    }
}
