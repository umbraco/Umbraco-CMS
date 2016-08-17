using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    class DocumentTypeContainerRepository : EntityContainerRepository, IDocumentTypeContainerRepository
    {
        public DocumentTypeContainerRepository(IDatabaseUnitOfWork uow, CacheHelper cache, ILogger logger, IMapperCollection mappers)
            : base(uow, cache, logger, mappers, Constants.ObjectTypes.DocumentTypeContainerGuid)
        { }
    }
}