using System;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents a service that works on top of repositories.
    /// </summary>
    public abstract class RepositoryService : IService
    {
        protected ILogger Logger { get; }
        protected IEventMessagesFactory EventMessagesFactory { get; }
        protected IScopeProvider ScopeProvider { get; }

        protected RepositoryService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            EventMessagesFactory = eventMessagesFactory ?? throw new ArgumentNullException(nameof(eventMessagesFactory));
            ScopeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        protected IQuery<T> Query<T>() => ScopeProvider.SqlContext.Query<T>();
    }
}
