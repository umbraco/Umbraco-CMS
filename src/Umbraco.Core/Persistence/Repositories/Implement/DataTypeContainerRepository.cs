using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    class DataTypeContainerRepository : EntityContainerRepository, IDataTypeContainerRepository
    {
        public DataTypeContainerRepository(IScopeUnitOfWork uow, CacheHelper cache, ILogger logger)
            : base(uow, cache, logger, Constants.ObjectTypes.DataTypeContainer)
        { }
    }
}
