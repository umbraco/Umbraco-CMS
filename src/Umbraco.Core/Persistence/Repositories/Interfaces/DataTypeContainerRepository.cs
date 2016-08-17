using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    class DataTypeContainerRepository : EntityContainerRepository, IDataTypeContainerRepository
    {
        public DataTypeContainerRepository(IDatabaseUnitOfWork uow, CacheHelper cache, ILogger logger, IMapperCollection mappers)
            : base(uow, cache, logger, mappers, Constants.ObjectTypes.DataTypeContainerGuid)
        { }
    }
}