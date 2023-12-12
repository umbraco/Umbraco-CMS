using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
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
    private readonly IUserRepository _userRepository;

    public UserGroupService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IUserGroupRepository userGroupRepository,
        IUserGroupPermissionService userGroupPermissionService,
        IEntityService entityService,
        IUserService userService,
        IUserRepository userRepository)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _userGroupRepository = userGroupRepository;
        _userGroupPermissionService = userGroupPermissionService;
        _entityService = entityService;
        _userService = userService;
        _userRepository = userRepository;
    }

    /// <inheritdoc/>
    public Task<PagedModel<IUserGroup>> GetAllAsync(int skip, int take)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope(autoComplete: true);
        IUserGroup[] groups = _userGroupRepository.GetMany()
            .OrderBy(x => x.Name).ToArray();

        var total = groups.Length;

        return Task.FromResult(new PagedModel<IUserGroup>
        {
            Items = groups.Skip(skip).Take(take),
            Total = total,
        });
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

        scope.Notifications.Publish(new UserGroupDeletedNotification(userGroupsToDelete, eventMessages).WithStateFrom(deletingNotification));

        scope.Complete();

        return Attempt.Succeed(UserGroupOperationStatus.Success);
    }

    public async Task UpdateUserGroupsOnUsers(
        ISet<Guid> userGroupKeys,
        ISet<Guid> userKeys)
    {
        IUser[] users = (await _userService.GetAsync(userKeys)).ToArray();

        IReadOnlyUserGroup[] userGroups = (await GetAsync(userGroupKeys))
            .Select(x => x.ToReadOnlyGroup())
            .ToArray();

        foreach(IUser user in users)
        {
            user.ClearGroups();
            foreach (IReadOnlyUserGroup userGroup in userGroups)
            {
                user.AddGroup(userGroup);
            }
        }

        _userService.Save(users);
    }

    private Attempt<UserGroupOperationStatus> ValidateUserGroupDeletion(IUserGroup? userGroup)
    {
        if (userGroup is null)
        {
            return Attempt.Fail(UserGroupOperationStatus.NotFound);
        }

        if (userGroup.IsSystemUserGroup())
        {
            return Attempt.Fail(UserGroupOperationStatus.IsSystemUserGroup);
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

        Attempt<IUserGroup, UserGroupOperationStatus> validationAttempt = await ValidateUserGroupCreationAsync(userGroup);
        if (validationAttempt.Success is false)
        {
            return validationAttempt;
        }

        UserGroupAuthorizationStatus isAuthorized = await _userGroupPermissionService.AuthorizeCreateAsync(performingUser, userGroup);
        if (isAuthorized != UserGroupAuthorizationStatus.Success)
        {
            // Convert from UserGroupAuthorizationStatus to UserGroupOperationStatus
            UserGroupOperationStatus operationStatus = isAuthorized.ToUserGroupOperationStatus();
            return Attempt.FailWithStatus(operationStatus, userGroup);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new UserGroupSavingNotification(userGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        Guid[] checkedGroupMembersKeys = EnsureNonAdminUserIsInSavedUserGroup(performingUser, groupMembersKeys ?? Enumerable.Empty<Guid>()).ToArray();
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

        // shouldn't we publish a UserGroupSavedNotification / UserGroupWithUsersSavedNotification here?
        scope.Complete();
        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    private async Task<Attempt<IUserGroup, UserGroupOperationStatus>> ValidateUserGroupCreationAsync(IUserGroup userGroup)
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
        Guid userKey,
        Guid[]? groupUserKeys)
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

        UserGroupAuthorizationStatus isAuthorized = await _userGroupPermissionService.AuthorizeUpdateAsync(performingUser, userGroup);
        if (isAuthorized != UserGroupAuthorizationStatus.Success)
        {
            // Convert from UserGroupAuthorizationStatus to UserGroupOperationStatus
            UserGroupOperationStatus operationStatus = isAuthorized.ToUserGroupOperationStatus();
            return Attempt.FailWithStatus(operationStatus, userGroup);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new UserGroupSavingNotification(userGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        _userGroupRepository.Save(userGroup);

        if (groupUserKeys is not null)
        {
            AssignUsersToUserGroup(groupUserKeys, userGroup, eventMessages, scope);
        }


        scope.Complete();
        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    private void AssignUsersToUserGroup(Guid[] groupUserKeys, IUserGroup userGroup, EventMessages eventMessages, ICoreScope scope)
    {
        // update the users with the right groups
        // we need to figure out which users have been added / removed, for audit purposes
        var groupUserIds = _userRepository
            .Get(ScopeProvider.CreateQuery<IUser>().Where(user => groupUserKeys.Contains(user.Key)))
            .Select(user => user.Id)
            .ToArray();
        IUser[] currentGroupUsers =
            userGroup.HasIdentity ? _userRepository.GetAllInGroup(userGroup.Id).ToArray() : Array.Empty<IUser>();
        var currentGroupUsersIdDict = currentGroupUsers.ToDictionary(x => x.Id, x => x);
        var existingGroupUserIds = currentGroupUsers.Select(x => x.Id).ToArray();
        var usersToAddIds = groupUserIds.Except(existingGroupUserIds).ToArray();

        IUser[] usersToAdd = usersToAddIds.Length is not 0
            ? _userRepository.Get(ScopeProvider.CreateQuery<IUser>().Where(user => usersToAddIds.Contains(user.Id))).ToArray()
            : Array.Empty<IUser>();
        IUser[] usersToRemove = existingGroupUserIds.Except(groupUserIds).Select(x => currentGroupUsersIdDict[x]).Where(x => x.Id != 0).ToArray();

        var userGroupWithUsers = new UserGroupWithUsers(userGroup, usersToAdd, usersToRemove);

        // this is an additional notification for special auditing
        var savingUserGroupWithUsersNotification =
            new UserGroupWithUsersSavingNotification(userGroupWithUsers, eventMessages);
        if (scope.Notifications.PublishCancelable(savingUserGroupWithUsersNotification))
        {
            scope.Complete();
            return;
        }

        _userGroupRepository.AddOrUpdateGroupWithUsers(userGroup, groupUserIds);

        scope.Notifications.Publish(new UserGroupWithUsersSavedNotification(userGroupWithUsers, eventMessages).WithStateFrom(savingUserGroupWithUsersNotification));
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

        IUserGroup? existing = _userGroupRepository.Get(userGroup.Alias);
        if (existing is not null && existing.Key != userGroup.Key)
        {
            return UserGroupOperationStatus.DuplicateAlias;
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

    /// <summary>
    /// Ensures that the user creating the user group is either an admin, or in the group itself.
    /// </summary>
    /// <remarks>
    /// This is to ensure that the user can access the group they themselves created at a later point and modify it.
    /// </remarks>
    private IEnumerable<Guid> EnsureNonAdminUserIsInSavedUserGroup(IUser performingUser, IEnumerable<Guid> groupMembersUserKeys)
    {
        var userKeys = groupMembersUserKeys.ToList();

        // If the performing user is an admin we don't care, they can access the group later regardless
        if (performingUser.IsAdmin() is false && userKeys.Contains(performingUser.Key) is false)
        {
            userKeys.Add(performingUser.Key);
        }

        return userKeys;
    }
}
