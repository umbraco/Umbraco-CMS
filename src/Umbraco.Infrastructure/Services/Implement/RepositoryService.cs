using System;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents a service that works on top of repositories.
    /// </summary>
    public abstract class RepositoryService : IService
    {
        protected IEventMessagesFactory EventMessagesFactory { get; }
        protected IScopeProvider ScopeProvider { get; }
        protected ILoggerFactory LoggerFactory { get; }

        protected RepositoryService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
        {
            EventMessagesFactory = eventMessagesFactory ?? throw new ArgumentNullException(nameof(eventMessagesFactory));
            ScopeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected IQuery<T> Query<T>() => ScopeProvider.SqlContext.Query<T>();
    }
}
