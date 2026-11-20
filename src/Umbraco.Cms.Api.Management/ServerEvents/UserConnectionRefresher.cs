using System.Security.Claims;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Refreshes the event source groups of the connections held by this server when a user is refreshed.
/// </summary>
/// <remarks>
/// Handles the user cache refresher notification rather than the user saved notification, so that every server
/// refreshes the connections it holds.
/// </remarks>
internal sealed class UserConnectionRefresher : INotificationAsyncHandler<UserCacheRefresherNotification>
{
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IServerEventUserManager _serverEventUserManager;
    private readonly IUserConnectionManager _userConnectionManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectionRefresher"/> class.
    /// </summary>
    /// <param name="backOfficeSignInManager">The service used to create a principal for a user.</param>
    /// <param name="serverEventUserManager">The manager that assigns a user's connections to event source groups.</param>
    /// <param name="userConnectionManager">Tracks the connections held by this server.</param>
    public UserConnectionRefresher(
        IBackOfficeSignInManager backOfficeSignInManager,
        IServerEventUserManager serverEventUserManager,
        IUserConnectionManager userConnectionManager)
    {
        _backOfficeSignInManager = backOfficeSignInManager;
        _serverEventUserManager = serverEventUserManager;
        _userConnectionManager = userConnectionManager;
    }

    /// <summary>
    /// Handles a <see cref="UserCacheRefresherNotification"/> by refreshing the group memberships of each refreshed user
    /// that has connections on this server.
    /// </summary>
    /// <param name="notification">The notification containing the refreshed users.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(UserCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not UserCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        foreach (Guid userKey in payloads.Select(x => x.Key).Distinct())
        {
            if (_userConnectionManager.GetConnections(userKey).Count == 0)
            {
                continue;
            }

            // This doesn't sign the user in, it only creates a principal to authorize against.
            ClaimsPrincipal? claimsPrincipal = await _backOfficeSignInManager.CreateUserPrincipalAsync(userKey);
            if (claimsPrincipal is null)
            {
                continue;
            }

            await _serverEventUserManager.RefreshGroupsAsync(claimsPrincipal);
        }
    }
}
