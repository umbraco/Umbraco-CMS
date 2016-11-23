using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    class DataTypeContainerRepository : EntityContainerRepository, IDataTypeContainerRepository
    {
        public DataTypeContainerRepository(IDatabaseUnitOfWork uow, CacheHelper cache, ILogger logger, IQueryFactory queryFactory)
            : base(uow, cache, logger, queryFactory, Constants.ObjectTypes.DataTypeContainerGuid)
        { }
    }
}