using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Filters recipients of an entity-scoped server event (for example document or media) for a
/// specific event source, based on whether a user may access the entity at a given tree path.
/// This is the broadcast-time counterpart to <see cref="IEventSourceAuthorizer"/>, which authorizes
/// access to a whole event source at connect time.
/// </summary>
public interface IServerEventEntityAccessFilter
{
    /// <summary>
    /// Gets the event sources this filter applies to.
    /// </summary>
    IEnumerable<string> FilteredEventSources { get; }

    /// <summary>
    /// Determines whether the user may receive an event described by the given routing context.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="context">The routing context describing the entity the event concerns (for example its tree path).</param>
    /// <returns><c>true</c> if the user may receive the event; otherwise <c>false</c>.</returns>
    Task<bool> HasAccessAsync(IUser user, ServerEventRoutingContext context);
}
