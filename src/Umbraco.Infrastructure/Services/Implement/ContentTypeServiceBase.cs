using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Implement
{
    public abstract class ContentTypeServiceBase : RepositoryService
    {
        protected ContentTypeServiceBase(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
            : base(provider, loggerFactory, eventMessagesFactory)
        { }
    }
}
