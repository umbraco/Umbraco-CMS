using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    class MediaTypeContainerRepository : EntityContainerRepository, IMediaTypeContainerRepository
    {
        public MediaTypeContainerRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MediaTypeContainerRepository> logger)
            : base(scopeAccessor, cache, logger, Cms.Core.Constants.ObjectTypes.MediaTypeContainer)
        { }
    }
}
