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
    /// <summary>
    ///     Initializes a new instance of the <see cref="RepositoryService" /> class.
    /// </summary>
    /// <param name="provider">The scope provider for unit of work operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    protected RepositoryService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
    {
        EventMessagesFactory = eventMessagesFactory ?? throw new ArgumentNullException(nameof(eventMessagesFactory));
        ScopeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    ///     Gets the event messages factory.
    /// </summary>
    protected IEventMessagesFactory EventMessagesFactory { get; }

    /// <summary>
    ///     Gets the scope provider.
    /// </summary>
    protected ICoreScopeProvider ScopeProvider { get; }

    /// <summary>
    ///     Gets the logger factory.
    /// </summary>
    protected ILoggerFactory LoggerFactory { get; }

    /// <summary>
    ///     Creates a query for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to query.</typeparam>
    /// <returns>A new query instance.</returns>
    protected IQuery<T> Query<T>() => ScopeProvider.CreateQuery<T>();
}
