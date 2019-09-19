using System;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public abstract class ScopeRepositoryService : RepositoryService
    {
        [Obsolete("Use the ctor specifying a IScopeUnitOfWorkProvider instead")]
        protected ScopeRepositoryService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory) 
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
            var scopeUow = provider as IScopeUnitOfWorkProvider;
            if (scopeUow == null) throw new NotSupportedException("The provider type passed in: " + provider.GetType() + " is not of type " + typeof(IScopeUnitOfWorkProvider));
            UowProvider = scopeUow;
        }

        protected ScopeRepositoryService(IScopeUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {            
            UowProvider = provider;
        }

        internal new IScopeUnitOfWorkProvider UowProvider { get; private set; }
    }
}