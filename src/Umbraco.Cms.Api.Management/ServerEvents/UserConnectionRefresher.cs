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

    public UserConnectionRefresher(
        IBackOfficeSignInManager backOfficeSignInManager,
        IServerEventUserManager serverEventUserManager)
    {
        _backOfficeSignInManager = backOfficeSignInManager;
        _serverEventUserManager = serverEventUserManager;
    }

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
