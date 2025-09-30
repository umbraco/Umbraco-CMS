using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc cref="Umbraco.Cms.Core.Services.IUserGroupService" />
internal sealed class UserGroupService : RepositoryService, IUserGroupService
{
    public const int MaxUserGroupNameLength = 200;
    public const int MaxUserGroupAliasLength = 200;

    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IUserGroupPermissionService _userGroupPermissionService;
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;
    private readonly ILogger<UserGroupService> _logger;

    public UserGroupService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IUserGroupRepository userGroupRepository,
        IUserGroupPermissionService userGroupPermissionService,
        IEntityService entityService,
        IUserService userService,
        ILogger<UserGroupService> logger)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _userGroupRepository = userGroupRepository;
        _userGroupPermissionService = userGroupPermissionService;
        _entityService = entityService;
        _userService = userService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<PagedModel<IUserGroup>> GetAllAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        IUserGroup[] groups = _userGroupRepository.GetMany()
            .OrderBy(x => x.Name).ToArray();

        var total = groups.Length;

        return Task.FromResult(new PagedModel<IUserGroup> { Items = groups.Skip(skip).Take(take), Total = total, });
    }

    /// <inheritdoc />
    public Task<IEnumerable<IUserGroup>> GetAsync(params int[] ids)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<IUserGroup> groups = _userGroupRepository
            .GetMany(ids)
            .OrderBy(x => x.Name);

        return Task.FromResult(groups);
    }

    /// <inheritdoc />
    public Task<IEnumerable<IUserGroup>> GetAsync(params string[] aliases)
    {
        if (aliases.Length == 0)
        {
            return Task.FromResult(Enumerable.Empty<IUserGroup>());
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IQuery<IUserGroup> query = Query<IUserGroup>().Where(x => aliases.SqlIn(x.Alias));
        IEnumerable<IUserGroup> contents = _userGroupRepository
            .Get(query)
            .WhereNotNull()
            .OrderBy(x => x.Name)
            .ToArray();

        return Task.FromResult(contents);
    }

    /// <inheritdoc />
    public Task<IUserGroup?> GetAsync(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IQuery<IUserGroup> query = Query<IUserGroup>().Where(x => x.Alias == alias);
        IUserGroup? contents = _userGroupRepository.Get(query).FirstOrDefault();
        return Task.FromResult(contents);
    }

    /// <inheritdoc />
    public Task<IUserGroup?> GetAsync(int id)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        return Task.FromResult(_userGroupRepository.Get(id));
    }

    /// <inheritdoc />
    public Task<IUserGroup?> GetAsync(Guid key)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IQuery<IUserGroup> query = Query<IUserGroup>().Where(x => x.Key == key);
        IUserGroup? groups = _userGroupRepository.Get(query).FirstOrDefault();
        return Task.FromResult(groups);
    }

    public Task<IEnumerable<IUserGroup>> GetAsync(IEnumerable<Guid> keys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);

        IQuery<IUserGroup> query = Query<IUserGroup>().Where(x => keys.SqlIn(x.Key));

        IUserGroup[] result = _userGroupRepository
            .Get(query)
            .WhereNotNull()
            .OrderBy(x => x.Name)
            .ToArray();

        return Task.FromResult<IEnumerable<IUserGroup>>(result);
    }

    /// <inheritdoc/>
    public async Task<Attempt<PagedModel<IUserGroup>, UserGroupOperationStatus>> FilterAsync(Guid userKey, string? filter, int skip, int take)
    {
        IUser? requestingUser = await _userService.GetAsync(userKey);
        if (requestingUser is null)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.MissingUser, new PagedModel<IUserGroup>());
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        var groups = _userGroupRepository
            .GetMany()
            .Where(group => filter.IsNullOrWhiteSpace() || group.Name?.InvariantContains(filter) is true)
            .OrderBy(group => group.Name)
            .ToList();

        if (requestingUser.IsAdmin() is false)
        {
            var requestingUserGroups = requestingUser.Groups.Select(group => group.Alias).ToArray();
            groups.RemoveAll(group =>
                group.Alias is Constants.Security.AdminGroupAlias
                || requestingUserGroups.Contains(group.Alias) is false);
        }

        return Attempt.SucceedWithStatus(
            UserGroupOperationStatus.Success,
            new PagedModel<IUserGroup> { Items = groups.Skip(skip).Take(take), Total = groups.Count });
    }

    /// <inheritdoc/>
    public async Task<Attempt<UserGroupOperationStatus>> DeleteAsync(ISet<Guid> keys)
    {
        if (keys.Any() is false)
        {
            return Attempt.Succeed(UserGroupOperationStatus.Success);
        }

        IUserGroup[] userGroupsToDelete = (await GetAsync(keys)).ToArray();

        if (userGroupsToDelete.Length != keys.Count)
        {
            return Attempt.Fail(UserGroupOperationStatus.NotFound);
        }

        foreach (IUserGroup userGroup in userGroupsToDelete)
        {
            Attempt<UserGroupOperationStatus> validationResult = ValidateUserGroupDeletion(userGroup);
            if (validationResult.Success is false)
            {
                return validationResult;
            }
        }

        using ICoreScope scope = ScopeProvider.CreateCoreScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        var deletingNotification = new UserGroupDeletingNotification(userGroupsToDelete, eventMessages);

        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return Attempt.Fail(UserGroupOperationStatus.CancelledByNotification);
        }

        foreach (IUserGroup userGroup in userGroupsToDelete)
        {
            _userGroupRepository.Delete(userGroup);
        }

        scope.Notifications.Publish(
            new UserGroupDeletedNotification(userGroupsToDelete, eventMessages).WithStateFrom(deletingNotification));

        scope.Complete();

        return Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    public async Task<Attempt<UserGroupOperationStatus>> UpdateUserGroupsOnUsersAsync(
        ISet<Guid> userGroupKeys,
        ISet<Guid> userKeys)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser[] users = (await _userService.GetAsync(userKeys)).ToArray();

        IReadOnlyUserGroup[] userGroups = (await GetAsync(userGroupKeys))
            .Select(x => x.ToReadOnlyGroup())
            .ToArray();

        // This means that we're potentially de-admining a user, which might cause the admin group to be empty.
        if (userGroupKeys.Contains(Constants.Security.AdminGroupKey) is false)
        {
            IUser[] usersToDeAdmin = users.Where(x => x.IsAdmin()).ToArray();
            if (usersToDeAdmin.Length > 0)
            {
                // Unfortunately we have to resolve the admin group to ensure that it would not be left empty.
                IUserGroup? adminGroup = await GetAsync(Constants.Security.AdminGroupKey);
                if (adminGroup is not null && adminGroup.UserCount <= usersToDeAdmin.Length)
                {
                    scope.Complete();
                    return Attempt.Fail(UserGroupOperationStatus.AdminGroupCannotBeEmpty);
                }
            }
        }

        foreach (IUser user in users)
        {
            user.ClearGroups();
            foreach (IReadOnlyUserGroup userGroup in userGroups)
            {
                user.AddGroup(userGroup);
            }
        }

        _userService.Save(users);

        scope.Complete();

        return Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    private Attempt<UserGroupOperationStatus> ValidateUserGroupDeletion(IUserGroup? userGroup)
    {
        if (userGroup is null)
        {
            return Attempt.Fail(UserGroupOperationStatus.NotFound);
        }

        if (userGroup.IsSystemUserGroup())
        {
            return Attempt.Fail(UserGroupOperationStatus.CanNotDeleteIsSystemUserGroup);
        }

        return Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(
        IUserGroup userGroup,
        Guid userKey,
        Guid[]? groupMembersKeys = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? performingUser = await _userService.GetAsync(userKey);
        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.MissingUser, userGroup);
        }

        Attempt<IUserGroup, UserGroupOperationStatus> validationAttempt =
            await ValidateUserGroupCreationAsync(userGroup);
        if (validationAttempt.Success is false)
        {
            return validationAttempt;
        }

        UserGroupAuthorizationStatus isAuthorized =
            await _userGroupPermissionService.AuthorizeCreateAsync(performingUser, userGroup);
        if (isAuthorized != UserGroupAuthorizationStatus.Success)
        {
            _logger.LogInformation("The performing user is not allowed to create the user group. The authorization status returned was: {AuthorizationStatus}", isAuthorized);
            return Attempt.FailWithStatus(UserGroupOperationStatus.Unauthorized, userGroup);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new UserGroupSavingNotification(userGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        Guid[] checkedGroupMembersKeys =
            EnsureNonAdminUserIsInSavedUserGroup(performingUser, groupMembersKeys ?? []);
        IUser[] usersToAdd = (await _userService.GetAsync(checkedGroupMembersKeys)).ToArray();

        // Since this is a brand new creation we don't have to be worried about what users were added and removed
        // simply put all members that are requested to be in the group will be "added"
        var userGroupWithUsers = new UserGroupWithUsers(userGroup, usersToAdd, Array.Empty<IUser>());
        var savingUserGroupWithUsersNotification = new UserGroupWithUsersSavingNotification(userGroupWithUsers, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingUserGroupWithUsersNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        _userGroupRepository.AddOrUpdateGroupWithUsers(userGroup, usersToAdd.Select(x => x.Id).ToArray());

        scope.Notifications.Publish(
            new UserGroupSavedNotification(userGroup, eventMessages).WithStateFrom(savingNotification));
        scope.Notifications.Publish(
            new UserGroupWithUsersSavedNotification(userGroupWithUsers, eventMessages).WithStateFrom(savingUserGroupWithUsersNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    private async Task<Attempt<IUserGroup, UserGroupOperationStatus>> ValidateUserGroupCreationAsync(
        IUserGroup userGroup)
    {
        if (await IsNewUserGroup(userGroup) is false)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.AlreadyExists, userGroup);
        }

        UserGroupOperationStatus commonValidationStatus = ValidateCommon(userGroup);
        if (commonValidationStatus != UserGroupOperationStatus.Success)
        {
            return Attempt.FailWithStatus(commonValidationStatus, userGroup);
        }

        if (_userGroupRepository.Get(userGroup.Alias) is not null)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.DuplicateAlias, userGroup);
        }

        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(
        IUserGroup userGroup,
        Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? performingUser = await _userService.GetAsync(userKey);
        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.MissingUser, userGroup);
        }

        UserGroupOperationStatus validationStatus = await ValidateUserGroupUpdateAsync(userGroup);
        if (validationStatus is not UserGroupOperationStatus.Success)
        {
            return Attempt.FailWithStatus(validationStatus, userGroup);
        }

        UserGroupAuthorizationStatus isAuthorized =
            await _userGroupPermissionService.AuthorizeUpdateAsync(performingUser, userGroup);
        if (isAuthorized != UserGroupAuthorizationStatus.Success)
        {
            _logger.LogInformation("The performing user is not allowed to update the user group. The authorization status returned was: {AuthorizationStatus}", isAuthorized);
            return Attempt.FailWithStatus(UserGroupOperationStatus.Unauthorized, userGroup);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new UserGroupSavingNotification(userGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        // We need to fire this notification - both for backwards compat, and to ensure caches across all servers.
        // Since we are not adding or removing any users, we'll just fire the notification with empty collections
        // for "added" and "removed" users.
        var userGroupWithUsers = new UserGroupWithUsers(userGroup, [], []);
        var savingUserGroupWithUsersNotification = new UserGroupWithUsersSavingNotification(userGroupWithUsers, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingUserGroupWithUsersNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        _userGroupRepository.Save(userGroup);

        scope.Notifications.Publish(
            new UserGroupSavedNotification(userGroup, eventMessages).WithStateFrom(savingNotification));
        scope.Notifications.Publish(
            new UserGroupWithUsersSavedNotification(userGroupWithUsers, eventMessages).WithStateFrom(savingUserGroupWithUsersNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    public async Task<Attempt<UserGroupOperationStatus>> AddUsersToUserGroupAsync(UsersToUserGroupManipulationModel addUsersModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        Attempt<ResolvedUserToUserGroupManipulationModel?, UserGroupOperationStatus> resolveAttempt = await ResolveUserGroupManipulationModel(addUsersModel, performingUserKey);

        if (resolveAttempt.Success is false)
        {
            return Attempt.Fail(resolveAttempt.Status);
        }

        ResolvedUserToUserGroupManipulationModel? resolvedModel = resolveAttempt.Result ??

            // This should never happen, but we need to check it to avoid null reference exceptions
            throw new InvalidOperationException("The resolved model should not be null.");

        IReadOnlyUserGroup readOnlyGroup = resolvedModel.UserGroup.ToReadOnlyGroup();

        foreach (IUser user in resolvedModel.Users)
        {
            user.AddGroup(readOnlyGroup);
        }

        _userService.Save(resolvedModel.Users);

        scope.Complete();

        return Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    public async Task<Attempt<UserGroupOperationStatus>> RemoveUsersFromUserGroupAsync(UsersToUserGroupManipulationModel removeUsersModel, Guid performingUserKey)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        Attempt<ResolvedUserToUserGroupManipulationModel?, UserGroupOperationStatus> resolveAttempt = await ResolveUserGroupManipulationModel(removeUsersModel, performingUserKey);

        if (resolveAttempt.Success is false)
        {
            return Attempt.Fail(resolveAttempt.Status);
        }

        ResolvedUserToUserGroupManipulationModel? resolvedModel = resolveAttempt.Result

        // This should never happen, but we need to check it to avoid null reference exceptions
            ?? throw new InvalidOperationException("The resolved model should not be null.");

        foreach (IUser user in resolvedModel.Users)
        {
            // We can't remove a user from a group they're not part of.
            if (user.Groups.Select(x => x.Key).Contains(resolvedModel.UserGroup.Key) is false)
            {
                return Attempt.Fail(UserGroupOperationStatus.UserNotInGroup);
            }

            user.RemoveGroup(resolvedModel.UserGroup.Alias);
        }

        // Ensure that that the admin group is never empty.
        // This would mean that you could never add a user to the admin group again, since you need to be part of the admin group to do so.
        if (resolvedModel.UserGroup.Key == Constants.Security.AdminGroupKey
            && resolvedModel.UserGroup.UserCount <= resolvedModel.Users.Length)
        {
            return Attempt.Fail(UserGroupOperationStatus.AdminGroupCannotBeEmpty);
        }

        _userService.Save(resolvedModel.Users);

        scope.Complete();

        return Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    /// <summary>
    /// Resolves the user group manipulation model keys into actual entities.
    /// Checks whether the performing user exists.
    /// Checks whether all users that are part of the manipulation exist.
    /// </summary>
    private async Task<Attempt<ResolvedUserToUserGroupManipulationModel?, UserGroupOperationStatus>> ResolveUserGroupManipulationModel(UsersToUserGroupManipulationModel model, Guid performingUserKey)
    {
        IUser? performingUser = await _userService.GetAsync(performingUserKey);
        if (performingUser is null)
        {
            return Attempt.FailWithStatus<ResolvedUserToUserGroupManipulationModel?, UserGroupOperationStatus>(UserGroupOperationStatus.MissingUser, null);
        }

        IUserGroup? existingUserGroup = await GetAsync(model.UserGroupKey);

        if (existingUserGroup is null)
        {
            return Attempt.FailWithStatus<ResolvedUserToUserGroupManipulationModel?, UserGroupOperationStatus>(UserGroupOperationStatus.NotFound, null);
        }

        IUser[] users = (await _userService.GetAsync(model.UserKeys)).ToArray();

        if (users.Length != model.UserKeys.Length)
        {
            return Attempt.FailWithStatus<ResolvedUserToUserGroupManipulationModel?, UserGroupOperationStatus>(UserGroupOperationStatus.UserNotFound, null);
        }

        var resolvedModel = new ResolvedUserToUserGroupManipulationModel
        {
            UserGroup = existingUserGroup,
            Users = users,
        };

        return Attempt.SucceedWithStatus<ResolvedUserToUserGroupManipulationModel?, UserGroupOperationStatus>(UserGroupOperationStatus.Success, resolvedModel);
    }

    private async Task<UserGroupOperationStatus> ValidateUserGroupUpdateAsync(IUserGroup userGroup)
    {
        UserGroupOperationStatus commonValidationStatus = ValidateCommon(userGroup);
        if (commonValidationStatus != UserGroupOperationStatus.Success)
        {
            return commonValidationStatus;
        }

        if (await IsNewUserGroup(userGroup))
        {
            return UserGroupOperationStatus.NotFound;
        }

        IUserGroup? existingByAlias = _userGroupRepository.Get(userGroup.Alias);
        if (existingByAlias is not null && existingByAlias.Key != userGroup.Key)
        {
            return UserGroupOperationStatus.DuplicateAlias;
        }

        IUserGroup? existingByKey = await GetAsync(userGroup.Key);
        if (existingByKey is not null && existingByKey.IsSystemUserGroup() && existingByKey.Alias != userGroup.Alias)
        {
            return UserGroupOperationStatus.CanNotUpdateAliasIsSystemUserGroup;
        }

        return UserGroupOperationStatus.Success;
    }

    /// <summary>
    /// Validate common user group properties, that are shared between update, create, etc.
    /// </summary>
    private UserGroupOperationStatus ValidateCommon(IUserGroup userGroup)
    {
        if (string.IsNullOrEmpty(userGroup.Name))
        {
            return UserGroupOperationStatus.MissingName;
        }

        if (userGroup.Name.Length > MaxUserGroupNameLength)
        {
            return UserGroupOperationStatus.NameTooLong;
        }

        if (userGroup.Alias.Length > MaxUserGroupAliasLength)
        {
            return UserGroupOperationStatus.AliasTooLong;
        }

        UserGroupOperationStatus startNodesValidationStatus = ValidateStartNodesExists(userGroup);
        if (startNodesValidationStatus is not UserGroupOperationStatus.Success)
        {
            return startNodesValidationStatus;
        }

        UserGroupOperationStatus granularPermissionsValidationStatus = ValidateGranularPermissionsExists(userGroup);
        if (granularPermissionsValidationStatus is not UserGroupOperationStatus.Success)
        {
            return granularPermissionsValidationStatus;
        }

        return UserGroupOperationStatus.Success;
    }

    private async Task<bool> IsNewUserGroup(IUserGroup userGroup)
    {
        if (userGroup.Id != 0 && userGroup.HasIdentity is false)
        {
            return false;
        }

        return await GetAsync(userGroup.Key) is null;
    }

    private UserGroupOperationStatus ValidateStartNodesExists(IUserGroup userGroup)
    {
        if (userGroup.StartContentId is not null
            && userGroup.StartContentId is not Constants.System.Root
            && _entityService.Exists(userGroup.StartContentId.Value, UmbracoObjectTypes.Document) is false)
        {
            return UserGroupOperationStatus.DocumentStartNodeKeyNotFound;
        }

        if (userGroup.StartMediaId is not null
            && userGroup.StartMediaId is not Constants.System.Root
            && _entityService.Exists(userGroup.StartMediaId.Value, UmbracoObjectTypes.Media) is false)
        {
            return UserGroupOperationStatus.MediaStartNodeKeyNotFound;
        }

        return UserGroupOperationStatus.Success;
    }

    private UserGroupOperationStatus ValidateGranularPermissionsExists(IUserGroup userGroup)
    {
        Guid[] documentKeys = userGroup.GranularPermissions
            .OfType<DocumentGranularPermission>()
            .Select(p => p.Key)
            .ToArray();

        if (documentKeys.Any() && _entityService.Exists(documentKeys) is false)
        {
            return UserGroupOperationStatus.DocumentPermissionKeyNotFound;
        }

        Guid[] documentTypeKeys = userGroup.GranularPermissions
            .OfType<DocumentPropertyValueGranularPermission>()
            .Select(p => p.Key)
            .ToArray();

        if (documentTypeKeys.Any() && _entityService.Exists(documentTypeKeys) is false)
        {
            return UserGroupOperationStatus.DocumentTypePermissionKeyNotFound;
        }

        return UserGroupOperationStatus.Success;
    }

    /// <summary>
    /// Ensures that the user creating the user group is either an admin, or in the group itself.
    /// </summary>
    /// <remarks>
    /// This is to ensure that the user can access the group they themselves created at a later point and modify it.
    /// </remarks>
    private static Guid[] EnsureNonAdminUserIsInSavedUserGroup(
        IUser performingUser,
        Guid[] groupMembersUserKeys)
    {
        // If the performing user is an admin we don't care, they can access the group later regardless
        if (performingUser.IsAdmin() is false && groupMembersUserKeys.Contains(performingUser.Key) is false)
        {
            return [..groupMembersUserKeys, performingUser.Key];
        }

        return groupMembersUserKeys;
    }
}
