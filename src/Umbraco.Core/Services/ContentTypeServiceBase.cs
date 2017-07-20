using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    internal abstract class ContentTypeServiceBase : ScopeRepositoryService
    {
        protected ContentTypeServiceBase(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        { }
    }
}
