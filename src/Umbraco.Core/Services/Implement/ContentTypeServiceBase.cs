using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    internal abstract class ContentTypeServiceBase : ScopeRepositoryService
    {
        protected ContentTypeServiceBase(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        { }
    }
}
