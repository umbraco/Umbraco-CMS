using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    class DocumentTypeContainerRepository : EntityContainerRepository, IDocumentTypeContainerRepository
    {
        public DocumentTypeContainerRepository(IScopeUnitOfWork uow, CacheHelper cache, ILogger logger)
            : base(uow, cache, logger, Constants.ObjectTypes.DocumentTypeContainerGuid)
        { }
    }
}