using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping.EFCore;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Represents a service that works on top of repositories.
/// </summary>
public abstract class AsyncRepositoryService : IService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncRepositoryService" /> class.
    /// </summary>
    /// <param name="provider">The <see cref="IScopeProvider" /> used for EF Core unit of work operations.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="eventMessagesFactory">The factory for creating event messages.</param>
    protected AsyncRepositoryService(IScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
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
    protected IScopeProvider ScopeProvider { get; }

    /// <summary>
    ///     Gets the logger factory.
    /// </summary>
    protected ILoggerFactory LoggerFactory { get; }
}
