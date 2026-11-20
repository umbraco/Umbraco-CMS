namespace Umbraco.Cms.Core.Models.ServerEvents;

/// <summary>
///     A server event to route, together with the server-side context used to decide who receives it.
/// </summary>
public sealed class ServerEventRoute
{
    /// <summary>
    ///     Gets the server event to route.
    /// </summary>
    public required ServerEvent ServerEvent { get; init; }

    /// <summary>
    ///     Gets the server-side routing context, or <c>null</c> to route the event to everyone allowed to see its source.
    ///     Not sent to clients.
    /// </summary>
    public ServerEventRoutingContext? Context { get; init; }
}
