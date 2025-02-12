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
