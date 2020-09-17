using Microsoft.Extensions.Logging;
using Umbraco.Core.Cache;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class DataTypeContainerRepository : EntityContainerRepository, IDataTypeContainerRepository
    {
        public DataTypeContainerRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<DataTypeContainerRepository> logger)
            : base(scopeAccessor, cache, logger, Constants.ObjectTypes.DataTypeContainer)
        { }
    }
}
