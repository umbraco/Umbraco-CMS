using System.Security.Claims;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Authorizes a Claims principal to access an event source.
/// </summary>
public interface IEventSourceAuthorizer
{
    public IEnumerable<string> AuthorizedEventSources { get; }

    Task<bool> AuthorizeAsync(ClaimsPrincipal principal, string eventSource);
}
