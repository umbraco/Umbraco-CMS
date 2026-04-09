using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Defines the server event hub contract for real-time communication with clients.
/// </summary>
/// <remarks>
/// This interface is implemented by SignalR hubs and defines the methods
/// that can be invoked on connected clients to push server events.
/// </remarks>
public interface IServerEventHub
{
    /// <summary>
    /// Notifies connected clients of a server event.
    /// </summary>
    /// <param name="payload">The server event payload to send to clients.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    /// <remarks>
    /// The method name is lowercase to match SignalR client-side conventions.
    /// </remarks>
#pragma warning disable SA1300
    Task notify(ServerEvent payload);
#pragma warning restore SA1300
}
