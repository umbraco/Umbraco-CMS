using System;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents a service that works on top of repositories.
    /// </summary>
    public abstract class RepositoryService : IService
    {
        protected ILogger Logger { get; }
        protected IEventMessagesFactory EventMessagesFactory { get; }
        protected IScopeUnitOfWorkProvider UowProvider { get; }

        protected RepositoryService(IScopeUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            EventMessagesFactory = eventMessagesFactory ?? throw new ArgumentNullException(nameof(eventMessagesFactory));
            UowProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        protected IQuery<T> Query<T>() => UowProvider.DatabaseContext.Query<T>();
    }
}