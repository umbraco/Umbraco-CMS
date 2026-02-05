using System.Security.Claims;
using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Provides authorization services for server-sent events.
/// </summary>
/// <remarks>
/// This service determines which event sources a user is authorized to subscribe to
/// based on their claims and the registered <see cref="IEventSourceAuthorizer"/> instances.
/// </remarks>
public interface IServerEventAuthorizationService
{
    /// <summary>
    /// Authorizes a user to listen to server events.
    /// </summary>
    /// <param name="user">The user to authorize.</param>
    /// <returns>The authorization result, containing all authorized event sources and unauthorized event sources.</returns>
    Task<SeverEventAuthorizationResult> AuthorizeAsync(ClaimsPrincipal user);
}
