using Microsoft.Extensions.Logging;
using Umbraco.Core.Cache;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    class MediaTypeContainerRepository : EntityContainerRepository, IMediaTypeContainerRepository
    {
        public MediaTypeContainerRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MediaTypeContainerRepository> logger)
            : base(scopeAccessor, cache, logger, Constants.ObjectTypes.MediaTypeContainer)
        { }
    }
}
