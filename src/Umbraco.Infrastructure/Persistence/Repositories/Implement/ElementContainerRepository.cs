using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Represents a repository for managing element containers (folders) in the persistence layer.
/// </summary>
internal sealed class ElementContainerRepository : EntityContainerRepository, IElementContainerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementContainerRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="cache">The application-level caches used for caching entities and query results.</param>
    /// <param name="logger">The logger instance for logging repository operations.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    public ElementContainerRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<ElementContainerRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            Constants.ObjectTypes.ElementContainer,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }
}
