using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class ElementContainerRepository : EntityContainerRepository, IElementContainerRepository
{
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
