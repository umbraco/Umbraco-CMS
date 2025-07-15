using System.Security.Claims;

namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// Manages group access for a user.
/// </summary>
public interface IServerEventUserManager
{
    /// <summary>
    /// Adds the connections to the groups that the user has access to.
    /// </summary>
    /// <param name="user">The owner of the connection.</param>
    /// <param name="connectionId">The connection to add to groups.</param>
    /// <returns></returns>
    Task AssignToGroupsAsync(ClaimsPrincipal user, string connectionId);

    /// <summary>
    /// Reauthorize the user and removes all connections held by the user from groups they are no longer allowed to access.
    /// </summary>
    /// <param name="user">The user to reauthorize.</param>
    /// <returns></returns>
    Task RefreshGroupsAsync(ClaimsPrincipal user);
}
