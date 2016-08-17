using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    class MediaTypeContainerRepository : EntityContainerRepository, IMediaTypeContainerRepository
    {
        public MediaTypeContainerRepository(IDatabaseUnitOfWork uow, CacheHelper cache, ILogger logger, IMappingResolver mappingResolver)
            : base(uow, cache, logger, mappingResolver, Constants.ObjectTypes.MediaTypeContainerGuid)
        { }
    }
}