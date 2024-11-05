using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
///     Implements <see cref="IDomainCache" /> for NuCache.
/// </summary>
public class DomainCache : IDomainCache
{
    private readonly IDomainCacheService _domainCacheService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainCache" /> class.
    /// </summary>
    public DomainCache(IDefaultCultureAccessor defaultCultureAccessor, IDomainCacheService domainCacheService)
    {
        _domainCacheService = domainCacheService;
        DefaultCulture = defaultCultureAccessor.DefaultCulture;
    }

    /// <inheritdoc />
    public string DefaultCulture { get; }

    /// <inheritdoc />
    public IEnumerable<Domain> GetAll(bool includeWildcards) => _domainCacheService.GetAll(includeWildcards);

    /// <inheritdoc />
    public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false) => _domainCacheService.GetAssigned(documentId, includeWildcards);

    /// <inheritdoc />
    public bool HasAssigned(int documentId, bool includeWildcards = false) => _domainCacheService.HasAssigned(documentId, includeWildcards);
}
