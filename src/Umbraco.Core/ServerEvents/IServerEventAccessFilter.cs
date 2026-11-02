using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Filters the recipients of a server event for a specific event source, based on whether a user may
/// access what the event concerns. This is the broadcast-time counterpart to
/// <see cref="IEventSourceAuthorizer"/>, which authorizes access to a whole event source at connect time.
/// </summary>
/// <remarks>
/// The built-in filters gate entity-scoped sources (document and media) by the recipient's start-node
/// access to the entity's tree path, but a filter may base its decision on anything available from the
/// user or the routing context.
/// </remarks>
public interface IServerEventAccessFilter
{
    /// <summary>
    /// Gets the event sources this filter applies to.
    /// </summary>
    IEnumerable<string> FilteredEventSources { get; }

    /// <summary>
    /// Determines whether the user may receive an event described by the given routing context.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="context">The routing context describing what the event concerns (for example the entity's tree path).</param>
    /// <returns><c>true</c> if the user may receive the event; otherwise <c>false</c>.</returns>
    Task<bool> HasAccessAsync(IUser user, ServerEventRoutingContext context);
}
