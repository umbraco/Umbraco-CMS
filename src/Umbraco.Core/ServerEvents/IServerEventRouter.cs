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
}
