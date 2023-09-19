using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class MemberTypeContainerRepository : EntityContainerRepository, IMemberTypeContainerRepository
    {
        public MemberTypeContainerRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MemberTypeContainerRepository> logger)
            : base(scopeAccessor, cache, logger, Constants.ObjectTypes.MemberTypeContainer)
        {
        }
    }
}
