using System.Security.Claims;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Authorizes a Claims principal to access an event source.
/// </summary>
public interface IEventSourceAuthorizer
{
    /// <summary>
    /// The event sources this authorizer authorizes for.
    /// </summary>
    public IEnumerable<string> AuthorizableEventSources { get; }

    /// <summary>
    /// Authorizes a Claims principal to access an event source.
    /// </summary>
    /// <param name="principal">The principal that being authorized.</param>
    /// <param name="eventSource">The event source to authorize the principal for.</param>
    /// <returns>True is authorized, false if unauthorized.</returns>
    Task<bool> AuthorizeAsync(ClaimsPrincipal principal, string eventSource);
}
