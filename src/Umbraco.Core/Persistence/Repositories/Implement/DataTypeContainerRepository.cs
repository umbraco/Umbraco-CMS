using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    class DataTypeContainerRepository : EntityContainerRepository, IDataTypeContainerRepository
    {
        public DataTypeContainerRepository(ScopeProvider scopeProvider, CacheHelper cache, ILogger logger)
            : base(scopeProvider, cache, logger, Constants.ObjectTypes.DataTypeContainer)
        { }
    }
}
