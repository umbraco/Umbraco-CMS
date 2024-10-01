using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class DatabaseCacheRebuilder : IDatabaseCacheRebuilder
{
    private readonly IDatabaseCacheRepository _databaseCacheRepository;

    public DatabaseCacheRebuilder(IDatabaseCacheRepository databaseCacheRepository)
    {
        _databaseCacheRepository = databaseCacheRepository;
    }

    public void Rebuild() => _databaseCacheRepository.Rebuild();
}
