using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Routes server events to the correct users.
/// </summary>
public interface IServerEventRouter
{
    /// <summary>
    /// Route a server event the users that has permissions to see it.
    /// </summary>
    /// <param name="serverEvent">The server event to route.</param>
    /// <returns></returns>
    Task RouteEventAsync(ServerEvent serverEvent);

    /// <summary>
    /// Route a server event to the users that have permission to see it, using the supplied routing
    /// context to gate delivery per recipient (e.g. by start-node access for document and media events).
    /// </summary>
    /// <param name="serverEvent">The server event to route.</param>
    /// <param name="context">Server-side routing context. Not sent to clients.</param>
    /// <returns>A task that represents the asynchronous routing operation.</returns>
    // TODO (V19): Remove this default implementation.
    Task RouteEventAsync(ServerEvent serverEvent, ServerEventRoutingContext context)
        => RouteEventAsync(serverEvent);

    /// <summary>
    /// Route several server events to the users that have permission to see them.
    /// </summary>
    /// <remarks>
    /// Use this when a single operation raises many events, so an implementation can dispatch them together
    /// rather than one at a time.
    /// </remarks>
    /// <param name="routes">The server events to route, each with its routing context.</param>
    /// <returns>A task that represents the asynchronous routing operation.</returns>
    // TODO (V20): Remove this default implementation.
    async Task RouteEventsAsync(IEnumerable<ServerEventRoute> routes)
    {
        foreach (ServerEventRoute route in routes)
        {
            await (route.Context is null
                ? RouteEventAsync(route.ServerEvent)
                : RouteEventAsync(route.ServerEvent, route.Context));
        }
    }

    /// <summary>
    /// Notify a specific user about a server event.
    /// <remarks>Does not consider authorization.</remarks>
    /// </summary>
    /// <param name="serverEvent">The server event to send to the user.</param>
    /// <param name="userKey">Key of the user.</param>
    /// <returns></returns>
    Task NotifyUserAsync(ServerEvent serverEvent, Guid userKey);

    /// <summary>
    /// Broadcast a server event to all users, regardless of authorization.
    /// </summary>
    /// <param name="serverEvent">The event to broadcast.</param>
    /// <returns></returns>
    Task BroadcastEventAsync(ServerEvent serverEvent);
}
