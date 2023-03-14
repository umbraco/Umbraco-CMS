using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class DocumentTypeContainerRepository : EntityContainerRepository, IDocumentTypeContainerRepository
{
    public DocumentTypeContainerRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<DataTypeContainerRepository> logger)
        : base(scopeAccessor, cache, logger, Constants.ObjectTypes.DocumentTypeContainer)
    {
    }
}
