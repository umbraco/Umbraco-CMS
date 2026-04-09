using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Base class for content type services, providing common repository service functionality.
/// </summary>
public abstract class ContentTypeServiceBase : RepositoryService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeServiceBase"/> class.
    /// </summary>
    /// <param name="provider">The core scope provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    protected ContentTypeServiceBase(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
    }
}
