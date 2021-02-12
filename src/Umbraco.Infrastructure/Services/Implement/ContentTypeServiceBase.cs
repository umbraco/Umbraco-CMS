using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public abstract class ContentTypeServiceBase : ScopeRepositoryService
    {
        protected ContentTypeServiceBase(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
            : base(provider, loggerFactory, eventMessagesFactory)
        { }
    }
}
