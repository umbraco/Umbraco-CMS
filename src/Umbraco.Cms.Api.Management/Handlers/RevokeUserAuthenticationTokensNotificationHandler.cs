using System.Data.Common;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Handlers;

internal sealed class RevokeUserAuthenticationTokensNotificationHandler :
        INotificationAsyncHandler<UserSavedNotification>,
        INotificationAsyncHandler<UserDeletedNotification>,
        INotificationAsyncHandler<UserGroupDeletingNotification>,
        INotificationAsyncHandler<UserLoginSuccessNotification>
{
    private readonly IUserService _userService;
    private readonly IUserGroupService _userGroupService;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly AppCaches _appCaches;
    private readonly ILogger<RevokeUserAuthenticationTokensNotificationHandler> _logger;
    private readonly SecuritySettings _securitySettings;

    public RevokeUserAuthenticationTokensNotificationHandler(
        IUserService userService,
        IUserGroupService userGroupService,
        IOpenIddictTokenManager tokenManager,
        AppCaches appCaches,
        ILogger<RevokeUserAuthenticationTokensNotificationHandler> logger,
        IOptions<SecuritySettings> securitySettingsOptions)
    {
        _userService = userService;
        _userGroupService = userGroupService;
        _tokenManager = tokenManager;
        _appCaches = appCaches;
        _logger = logger;
        _securitySettings = securitySettingsOptions.Value;
    }

    public async Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            foreach (IUser user in notification.SavedEntities)
            {
                // Flush the start node caches when editing a user
                user.FlushStartNodeCaches(_appCaches);

                if (user.IsSuper())
                {
                    continue;
                }

                // When a user is locked out and/or un-approved, make sure we revoke all tokens
                if (user.IsLockedOut || user.IsApproved is false)
                {
                    await RevokeTokensAsync(user);
                }
            }
        }
        catch (DbException e)
        {
            _logger.LogWarning(e, "This is expected when we upgrade from < Umbraco 14. Otherwise it should not happen");
        }
    }

    // We can only delete non-logged in users in Umbraco, meaning that such will not have a token,
    // so this is just a precaution in case this workflow changes in the future
    public async Task HandleAsync(UserDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IUser user in notification.DeletedEntities)
        {
            await RevokeTokensAsync(user);
        }
    }

    public async Task HandleAsync(UserGroupDeletingNotification notification, CancellationToken cancellationToken)
    {
        foreach (IUserGroup userGroup in notification.DeletedEntities)
        {
            IEnumerable<IUser>? users = await GetUsersByGroupKeyAsync(userGroup.Key);
            if (users is null)
            {
                continue;
            }

            // Flush the start node caches for all affected users when editing a group
            foreach (IUser user in users)
            {
                user.FlushStartNodeCaches(_appCaches);
            }
        }
    }

    public async Task HandleAsync(UserLoginSuccessNotification notification, CancellationToken cancellationToken)
    {
        if (_securitySettings.AllowConcurrentLogins is false)
        {
            var userId = notification.AffectedUserId;
            IUser? user = userId is not null ? await FindUserFromString(userId) : null;

            if (user is null)
            {
                return;
            }

            await RevokeTokensAsync(user);
        }
    }

    // Get data about the users part of a group before deleting it
    private async Task<IEnumerable<IUser>?> GetUsersByGroupKeyAsync(Guid userGroupKey)
    {
        IUserGroup? userGroup = await _userGroupService.GetAsync(userGroupKey);

        return userGroup is null
            ? null
            : _userService.GetAllInGroup(userGroup.Id);
    }

    private async Task RevokeTokensAsync(IUser user)
    {
        _logger.LogInformation("Revoking active tokens for user with ID {id}", user.Id);

        await _tokenManager.RevokeUmbracoUserTokens(user.Key);
    }

    private async Task<IUser?> FindUserFromString(string userId)
    {
        if (int.TryParse(userId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            return _userService.GetUserById(id);
        }

        // We couldn't directly convert the ID to an int, this is because the user logged in with external login.
        // So we need to look up the user by key.
        if (Guid.TryParse(userId, out Guid key))
        {
            return await _userService.GetAsync(key);
        }

        return null;
    }
}
