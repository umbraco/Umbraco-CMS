using System.Security.Claims;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// updates the user's connections if any, when a user is saved
/// </summary>
internal sealed class UserConnectionRefresher : INotificationAsyncHandler<UserSavedNotification>
{
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IServerEventUserManager _serverEventUserManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserConnectionRefresher"/> class, responsible for managing user connection refresh events in the back office.
    /// </summary>
    /// <param name="backOfficeSignInManager">The service responsible for handling back office user sign-in operations.</param>
    /// <param name="serverEventUserManager">The manager that handles user-related server events.</param>
    public UserConnectionRefresher(
        IBackOfficeSignInManager backOfficeSignInManager,
        IServerEventUserManager serverEventUserManager)
    {
        _backOfficeSignInManager = backOfficeSignInManager;
        _serverEventUserManager = serverEventUserManager;
    }

    /// <summary>
    /// Handles a <see cref="UserSavedNotification"/> by refreshing the group memberships for each saved user.
    /// </summary>
    /// <param name="notification">The notification containing the saved user entities.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IUser user in notification.SavedEntities)
        {
            // This might look strange, but we need a claims principal to authorize, this doesn't log the user in, but just creates a principal.
            ClaimsPrincipal? claimsIdentity = await _backOfficeSignInManager.CreateUserPrincipalAsync(user.Key);
            if (claimsIdentity is null)
            {
                return;
            }

            await _serverEventUserManager.RefreshGroupsAsync(claimsIdentity);
        }

    }
}
