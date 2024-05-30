using System.Data.Common;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Handlers;

internal sealed class RevokeUserAuthenticationTokensNotificationHandler :
        INotificationAsyncHandler<UserSavingNotification>,
        INotificationAsyncHandler<UserSavedNotification>,
        INotificationAsyncHandler<UserDeletedNotification>,
        INotificationAsyncHandler<UserGroupDeletingNotification>,
        INotificationAsyncHandler<UserGroupDeletedNotification>,
        INotificationAsyncHandler<UserLoginSuccessNotification>
{
    private const string NotificationStateKey = "Umbraco.Cms.Api.Management.Handlers.RevokeUserAuthenticationTokensNotificationHandler";

    private readonly IUserService _userService;
    private readonly IUserGroupService _userGroupService;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly ILogger<RevokeUserAuthenticationTokensNotificationHandler> _logger;
    private readonly SecuritySettings _securitySettings;

    public RevokeUserAuthenticationTokensNotificationHandler(
        IUserService userService,
        IUserGroupService userGroupService,
        IOpenIddictTokenManager tokenManager,
        ILogger<RevokeUserAuthenticationTokensNotificationHandler> logger,
        IOptions<SecuritySettings> securitySettingsOptions)
    {
        _userService = userService;
        _userGroupService = userGroupService;
        _tokenManager = tokenManager;
        _logger = logger;
        _securitySettings = securitySettingsOptions.Value;
    }

    // We need to know the pre-saving state of the saved users in order to compare if their access has changed
    public async Task HandleAsync(UserSavingNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            var usersAccess = new Dictionary<Guid, UserStartNodesAndGroupAccess>();
            foreach (IUser user in notification.SavedEntities)
            {
                UserStartNodesAndGroupAccess? priorUserAccess = await GetRelevantUserAccessDataByUserKeyAsync(user.Key);
                if (priorUserAccess == null)
                {
                    continue;
                }

                usersAccess.Add(user.Key, priorUserAccess);
            }

            notification.State[NotificationStateKey] = usersAccess;
        }
        catch (DbException e)
        {
            _logger.LogWarning(e, "This is expected when we upgrade from < Umbraco 14. Otherwise it should not happen");
        }
    }

    public async Task HandleAsync(UserSavedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            Dictionary<Guid, UserStartNodesAndGroupAccess>? preSavingUsersState = null;

            if (notification.State.TryGetValue(NotificationStateKey, out var value))
            {
                preSavingUsersState = value as Dictionary<Guid, UserStartNodesAndGroupAccess>;
            }

            // If we have a new user, there is no token
            if (preSavingUsersState is null || preSavingUsersState.Count == 0)
            {
                return;
            }

            foreach (IUser user in notification.SavedEntities)
            {
                if (user.IsSuper())
                {
                    continue;
                }

                // When a user is locked out and/or un-approved, make sure we revoke all tokens
                if (user.IsLockedOut || user.IsApproved is false)
                {
                    await RevokeTokensAsync(user);
                    continue;
                }

                // Don't revoke admin tokens to prevent log out when accidental changes
                if (user.IsAdmin())
                {
                    continue;
                }

                // Check if the user access has changed - we also need to revoke all tokens in this case
                if (preSavingUsersState.TryGetValue(user.Key, out UserStartNodesAndGroupAccess? preSavingState))
                {
                    UserStartNodesAndGroupAccess postSavingState = MapToUserStartNodesAndGroupAccess(user);
                    if (preSavingState.CompareAccess(postSavingState) == false)
                    {
                        await RevokeTokensAsync(user);
                    }
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

    // We need to know the pre-deleting state of the users part of the deleted group to revoke their tokens
    public async Task HandleAsync(UserGroupDeletingNotification notification, CancellationToken cancellationToken)
    {
        var usersInGroups = new Dictionary<Guid, IEnumerable<IUser>>();
        foreach (IUserGroup userGroup in notification.DeletedEntities)
        {
            var users = await GetUsersByGroupKeyAsync(userGroup.Key);
            if (users == null)
            {
                continue;
            }

            usersInGroups.Add(userGroup.Key, users);
        }

        notification.State[NotificationStateKey] = usersInGroups;
    }

    public async Task HandleAsync(UserGroupDeletedNotification notification, CancellationToken cancellationToken)
    {
        Dictionary<Guid, IEnumerable<IUser>>? preDeletingUsersInGroups = null;

        if (notification.State.TryGetValue(NotificationStateKey, out var value))
        {
            preDeletingUsersInGroups = value as Dictionary<Guid, IEnumerable<IUser>>;
        }

        if (preDeletingUsersInGroups is null)
        {
            return;
        }

        // since the user group was deleted, we can only use the information we collected before the deletion
        // this means that we will not be able to detect users in any groups that were eventually deleted (due to implementor/3th party supplier interference)
        // that were not in the initial to be deleted list
        foreach (IUser user in preDeletingUsersInGroups
                     .Where(group => notification.DeletedEntities.Any(entity => group.Key == entity.Key))
                     .SelectMany(group => group.Value))
        {
            await RevokeTokensAsync(user);
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

    // Get data about the user before saving
    private async Task<UserStartNodesAndGroupAccess?> GetRelevantUserAccessDataByUserKeyAsync(Guid userKey)
    {
        IUser? user = await _userService.GetAsync(userKey);

        return user is null
            ? null
            : MapToUserStartNodesAndGroupAccess(user);
    }

    private UserStartNodesAndGroupAccess MapToUserStartNodesAndGroupAccess(IUser user)
        => new(user.Groups.Select(g => g.Key), user.StartContentIds, user.StartMediaIds);

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

    private class UserStartNodesAndGroupAccess
    {
        public IEnumerable<Guid> GroupKeys { get; }

        public int[]? StartContentIds { get; }

        public int[]? StartMediaIds { get; }

        public UserStartNodesAndGroupAccess(IEnumerable<Guid> groupKeys, int[]? startContentIds, int[]? startMediaIds)
        {
            GroupKeys = groupKeys;
            StartContentIds = startContentIds;
            StartMediaIds = startMediaIds;
        }

        public bool CompareAccess(UserStartNodesAndGroupAccess other)
        {
            var areContentStartNodesEqual = (StartContentIds == null && other.StartContentIds == null) ||
                                            (StartContentIds != null && other.StartContentIds != null &&
                                            StartContentIds.SequenceEqual(other.StartContentIds));

            var areMediaStartNodesEqual = (StartMediaIds == null && other.StartMediaIds == null) ||
                                          (StartMediaIds != null && other.StartMediaIds != null &&
                                          StartMediaIds.SequenceEqual(other.StartMediaIds));

            return areContentStartNodesEqual &&
                   areMediaStartNodesEqual &&
                   GroupKeys.SequenceEqual(other.GroupKeys);
        }
    }
}
