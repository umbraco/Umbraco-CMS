using System.Security.Claims;
using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

public interface IServerEventAuthorizationService
{
    /// <summary>
    /// Authorizes a user to listen to server events.
    /// </summary>
    /// <param name="user">The user to authorize.</param>
    /// <returns>The authorization result, containing all authorized event sources, and unauthorized event sources.</returns>
    Task<SeverEventAuthorizationResult> AuthorizeAsync(ClaimsPrincipal user);
}
