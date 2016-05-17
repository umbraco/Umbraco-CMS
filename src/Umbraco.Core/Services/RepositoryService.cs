using System;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents a service that works on top of repositories.
    /// </summary>
    public abstract class RepositoryService : IService
    {
        protected ILogger Logger { get; private set; }
        protected IEventMessagesFactory EventMessagesFactory { get; private set; }
        protected IDatabaseUnitOfWorkProvider UowProvider { get; private set; }
        
        protected RepositoryService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (eventMessagesFactory == null) throw new ArgumentNullException(nameof(eventMessagesFactory));
            Logger = logger;
            EventMessagesFactory = eventMessagesFactory;
            UowProvider = provider;
        }
    }
}