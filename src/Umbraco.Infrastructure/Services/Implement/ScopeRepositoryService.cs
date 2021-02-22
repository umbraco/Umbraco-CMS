using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Core.Events;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    // TODO: that one does not add anything = kill
    public abstract class ScopeRepositoryService : RepositoryService
    {
        protected ScopeRepositoryService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
            : base(provider, loggerFactory, eventMessagesFactory)
        { }
    }
}
