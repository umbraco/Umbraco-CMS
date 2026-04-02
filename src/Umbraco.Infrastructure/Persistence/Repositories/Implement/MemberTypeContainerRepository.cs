using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class MemberTypeContainerRepository : EntityContainerRepository, IMemberTypeContainerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeContainerRepository"/> class, responsible for managing member type containers in the persistence layer.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for transactional operations.</param>
    /// <param name="cache">The application-level caches used for optimizing data retrieval and storage.</param>
    /// <param name="logger">The logger instance used for logging repository operations and errors.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning to ensure cache consistency.</param>
    /// <param name="cacheSyncService">Service used to synchronize cache across distributed environments.</param>
    public MemberTypeContainerRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<MemberTypeContainerRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            Constants.ObjectTypes.MemberTypeContainer,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }
}
