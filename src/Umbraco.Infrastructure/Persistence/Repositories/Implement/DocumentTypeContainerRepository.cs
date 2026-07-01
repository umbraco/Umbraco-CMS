using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class DocumentTypeContainerRepository : EntityContainerRepository, IDocumentTypeContainerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.DocumentTypeContainerRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="cache">The application-level caches used for storing and retrieving cached data.</param>
    /// <param name="logger">The logger used for logging repository-related events and errors.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public DocumentTypeContainerRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<DocumentTypeContainerRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            Constants.ObjectTypes.DocumentTypeContainer,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }
}
