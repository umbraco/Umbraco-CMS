using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents a service that works on top of repositories.
/// </summary>
public abstract class RepositoryService : IService
{
    protected RepositoryService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
    {
        EventMessagesFactory = eventMessagesFactory ?? throw new ArgumentNullException(nameof(eventMessagesFactory));
        ScopeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    protected IEventMessagesFactory EventMessagesFactory { get; }

    protected ICoreScopeProvider ScopeProvider { get; }

    protected ILoggerFactory LoggerFactory { get; }

    protected IQuery<T> Query<T>() => ScopeProvider.CreateQuery<T>();
}
