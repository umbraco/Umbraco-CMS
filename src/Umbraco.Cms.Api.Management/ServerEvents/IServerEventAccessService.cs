using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Resolves which of the currently connected users may receive a server event, by delegating the
/// per-source access decision to the registered
/// <see cref="Core.ServerEvents.IServerEventAccessFilter"/> instances.
/// This is the broadcast-time counterpart to
/// <see cref="Core.ServerEvents.IServerEventAuthorizationService"/>.
/// </summary>
internal interface IServerEventAccessService
{
    /// <summary>
    /// Gets whether any registered filter gates the given event source (i.e. the source has a
    /// per-recipient access boundary that must be honoured before broadcasting).
    /// </summary>
    /// <param name="eventSource">The event source to check.</param>
    /// <returns><c>true</c> if events for this source must be filtered per recipient; otherwise <c>false</c>.</returns>
    bool AppliesTo(string eventSource);

    /// <summary>
    /// Returns the connection ids of the users allowed to receive an event for the given source and routing context.
    /// </summary>
    /// <param name="eventSource">The event source.</param>
    /// <param name="context">The routing context describing what the event concerns.</param>
    /// <returns>The connection ids that should receive the event.</returns>
    Task<IReadOnlyList<string>> GetAuthorizedConnectionsAsync(string eventSource, ServerEventRoutingContext context);
}
