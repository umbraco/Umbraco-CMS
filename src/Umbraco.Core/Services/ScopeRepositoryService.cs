using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public abstract class ScopeRepositoryService : RepositoryService
    {
        protected ScopeRepositoryService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
            UowProvider = provider;
        }

        internal new IScopeUnitOfWorkProvider UowProvider { get; }
    }
}
