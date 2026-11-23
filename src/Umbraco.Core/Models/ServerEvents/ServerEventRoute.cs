namespace Umbraco.Cms.Core.Models.ServerEvents;

/// <summary>
///     A server event to route, together with what decides who receives it: a routing context, or the user to notify.
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

    /// <summary>
    ///     Gets the key of the user to send the event to regardless of authorization, or <c>null</c> to route the event
    ///     by its source. When set, <see cref="Context"/> is not used.
    /// </summary>
    public Guid? UserKey { get; init; }
}
