using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
///     Implements <see cref="IDomainCache" /> for NuCache.
/// </summary>
public class DomainCache : IDomainCache
{
    private readonly IDomainCacheService _domainCacheService;
    private readonly IDefaultCultureAccessor _defaultCultureAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainCache" /> class.
    /// </summary>
    /// <param name="defaultCultureAccessor">The accessor used to resolve the site's default culture.</param>
    /// <param name="domainCacheService">The service providing the configured domains.</param>
    /// <param name="runtimeState">Ignored. Retained for backwards compatibility.</param>
    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 20.")]
    public DomainCache(
        IDefaultCultureAccessor defaultCultureAccessor,
        IDomainCacheService domainCacheService,
#pragma warning disable IDE0060 // Remove unused parameter
        IRuntimeState runtimeState)
#pragma warning restore IDE0060 // Remove unused parameter
        : this(defaultCultureAccessor, domainCacheService)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainCache" /> class.
    /// </summary>
    /// <param name="defaultCultureAccessor">The accessor used to resolve the site's default culture.</param>
    /// <param name="domainCacheService">The service providing the configured domains.</param>
    public DomainCache(IDefaultCultureAccessor defaultCultureAccessor, IDomainCacheService domainCacheService)
    {
        _defaultCultureAccessor = defaultCultureAccessor;
        _domainCacheService = domainCacheService;
    }

    /// <inheritdoc />
    public string DefaultCulture => _defaultCultureAccessor.DefaultCulture;

    /// <inheritdoc />
    public IEnumerable<Domain> GetAll(bool includeWildcards) => _domainCacheService.GetAll(includeWildcards);

    /// <inheritdoc />
    public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false) => _domainCacheService.GetAssigned(documentId, includeWildcards);

    /// <inheritdoc />
    public bool HasAssigned(int documentId, bool includeWildcards = false) => _domainCacheService.HasAssigned(documentId, includeWildcards);
}
