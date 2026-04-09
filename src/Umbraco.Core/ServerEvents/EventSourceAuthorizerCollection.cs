using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Represents a collection of <see cref="IEventSourceAuthorizer"/> instances.
/// </summary>
/// <remarks>
/// This collection contains all registered event source authorizers that determine
/// whether users have permission to subscribe to specific server-sent event sources.
/// </remarks>
public class EventSourceAuthorizerCollection : BuilderCollectionBase<IEventSourceAuthorizer>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourceAuthorizerCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that provides the event source authorizers.</param>
    public EventSourceAuthorizerCollection(Func<IEnumerable<IEventSourceAuthorizer>> items)
        : base(items)
    {
    }
}
