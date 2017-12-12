using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    class MediaTypeContainerRepository : EntityContainerRepository, IMediaTypeContainerRepository
    {
        public MediaTypeContainerRepository(ScopeProvider scopeProvider, CacheHelper cache, ILogger logger)
            : base(scopeProvider, cache, logger, Constants.ObjectTypes.MediaTypeContainer)
        { }
    }
}
