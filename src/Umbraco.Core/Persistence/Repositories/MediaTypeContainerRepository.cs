using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    class MediaTypeContainerRepository : EntityContainerRepository, IMediaTypeContainerRepository
    {
        public MediaTypeContainerRepository(IDatabaseUnitOfWork uow, CacheHelper cache, ILogger logger)
            : base(uow, cache, logger, Constants.ObjectTypes.MediaTypeContainerGuid)
        { }
    }
}