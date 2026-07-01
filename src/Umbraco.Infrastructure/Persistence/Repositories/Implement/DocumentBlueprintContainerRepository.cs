using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class DocumentBlueprintContainerRepository : EntityContainerRepository, IDocumentBlueprintContainerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.DocumentBlueprintContainerRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="cache">The application-level caches used for optimizing data retrieval.</param>
    /// <param name="logger">The logger used for logging repository events and errors.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public DocumentBlueprintContainerRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<DocumentBlueprintContainerRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            Constants.ObjectTypes.DocumentBlueprintContainer,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }
}
