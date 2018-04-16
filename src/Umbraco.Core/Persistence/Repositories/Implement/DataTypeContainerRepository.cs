using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    class DataTypeContainerRepository : EntityContainerRepository, IDataTypeContainerRepository
    {
        public DataTypeContainerRepository(IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
            : base(scopeAccessor, cache, logger, Constants.ObjectTypes.DataTypeContainer)
        { }
    }
}
