using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class DocumentBlueprintContainerRepository : EntityContainerRepository, IDocumentBlueprintContainerRepository
{
    public DocumentBlueprintContainerRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<DocumentBlueprintContainerRepository> logger)
        : base(scopeAccessor, cache, logger, Constants.ObjectTypes.DocumentBlueprintContainer)
    {
    }
}
