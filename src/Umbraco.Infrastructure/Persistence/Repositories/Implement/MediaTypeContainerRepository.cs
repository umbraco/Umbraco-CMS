using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class MediaTypeContainerRepository : EntityContainerRepository, IMediaTypeContainerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeContainerRepository"/> class, which is responsible for managing media type containers in the persistence layer.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="cache">The application-level caches used for optimizing data retrieval and storage.</param>
    /// <param name="logger">The logger used for logging repository-related events and errors.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning for repository data.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public MediaTypeContainerRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<MediaTypeContainerRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            Constants.ObjectTypes.MediaTypeContainer,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }
}
