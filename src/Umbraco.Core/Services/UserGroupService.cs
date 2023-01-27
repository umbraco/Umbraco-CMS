using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

internal sealed class UserGroupService : RepositoryService, IUserGroupService
{
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IUserGroupAuthorizationService _userGroupAuthorizationService;
    private readonly IUserService _userService;

    public UserGroupService(
        ICoreScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IUserGroupRepository userGroupRepository,
        IUserGroupAuthorizationService userGroupAuthorizationService,
        IUserService userService)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _userGroupRepository = userGroupRepository;
        _userGroupAuthorizationService = userGroupAuthorizationService;
        _userService = userService;
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
            .ToArray();

        return Task.FromResult(contents);
    }

    /// <inheritdoc />
    public Task<IUserGroup?> GetAsync(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "alias");
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

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(
        IUserGroup userGroup,
        int performingUserId,
        int[]? groupMembersUserIds = null)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? performingUser = _userService.GetUserById(performingUserId);
        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.MissingUser, userGroup);
        }

        Attempt<IUserGroup, UserGroupOperationStatus> validationAttempt = await ValidateUserGroupCreationAsync(userGroup);
        if (validationAttempt.Success is false)
        {
            return validationAttempt;
        }

        Attempt<UserGroupOperationStatus> authorizationAttempt = _userGroupAuthorizationService.AuthorizeUserGroupCreation(performingUser, userGroup);
        if (authorizationAttempt.Success is false)
        {
            return Attempt.FailWithStatus(authorizationAttempt.Result, userGroup);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new UserGroupSavingNotification(userGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        var checkedGroupMembers = EnsureNonAdminUserIsInSavedUserGroup(performingUser, groupMembersUserIds ?? Enumerable.Empty<int>()).ToArray();
        IEnumerable<IUser> usersToAdd = _userService.GetUsersById(checkedGroupMembers);

        // Since this is a brand new creation we don't have to be worried about what users were added and removed
        // simply put all members that are requested to be in the group will be "added"
        var userGroupWithUsers = new UserGroupWithUsers(userGroup, usersToAdd.ToArray(), Array.Empty<IUser>());
        var savingUserGroupWithUsersNotification = new UserGroupWithUsersSavingNotification(userGroupWithUsers, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingUserGroupWithUsersNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        _userGroupRepository.AddOrUpdateGroupWithUsers(userGroup, checkedGroupMembers);

        scope.Complete();
        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    /// <inheritdoc />
    public async Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(
        IUserGroup userGroup,
        int performingUserId)
    {
        using ICoreScope scope = ScopeProvider.CreateCoreScope();

        IUser? performingUser = _userService.GetUserById(performingUserId);
        if (performingUser is null)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.MissingUser, userGroup);
        }

        Attempt<IUserGroup, UserGroupOperationStatus> validationAttempt = await ValidateUserGroupUpdateAsync(userGroup);
        if (validationAttempt.Success is false)
        {
            return validationAttempt;
        }

        Attempt<UserGroupOperationStatus> authorizationAttempt = _userGroupAuthorizationService.AuthorizeUserGroupUpdate(performingUser, userGroup);
        if (authorizationAttempt.Success is false)
        {
            return Attempt.FailWithStatus(authorizationAttempt.Result, userGroup);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new UserGroupSavingNotification(userGroup, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus(UserGroupOperationStatus.CancelledByNotification, userGroup);
        }

        _userGroupRepository.Save(userGroup);
        scope.Notifications.Publish(new UserGroupSavedNotification(userGroup, eventMessages).WithStateFrom(savingNotification));

        scope.Complete();
        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }


    private async Task<Attempt<IUserGroup, UserGroupOperationStatus>> ValidateUserGroupCreationAsync(IUserGroup userGroup)
    {
        if (await IsNewUserGroup(userGroup) is false)
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.AlreadyExists, userGroup);
        }

        return UserGroupHasUniqueAlias(userGroup) is false
            ? Attempt.FailWithStatus(UserGroupOperationStatus.DuplicateAlias, userGroup)
            : Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    private async Task<Attempt<IUserGroup, UserGroupOperationStatus>> ValidateUserGroupUpdateAsync(IUserGroup userGroup)
    {
        if (await IsNewUserGroup(userGroup))
        {
            return Attempt.FailWithStatus(UserGroupOperationStatus.NotFound, userGroup);
        }

        return Attempt.SucceedWithStatus(UserGroupOperationStatus.Success, userGroup);
    }

    private async Task<bool> IsNewUserGroup(IUserGroup userGroup)
    {
        if (userGroup.Id != 0 && userGroup.HasIdentity is false)
        {
            return false;
        }

        return await GetAsync(userGroup.Key) is null;
    }

    private bool UserGroupHasUniqueAlias(IUserGroup userGroup) => _userGroupRepository.Get(userGroup.Alias) is null;

    /// <summary>
    /// Ensures that the user creating the user group is either an admin, or in the group itself.
    /// </summary>
    /// <remarks>
    /// This is to ensure that the user can access the group they themselves created at a later point and modify it.
    /// </remarks>
    private IEnumerable<int> EnsureNonAdminUserIsInSavedUserGroup(IUser performingUser, IEnumerable<int> groupMembersUserIds)
    {
        var userIds = groupMembersUserIds.ToList();

        // If the performing user is and admin we don't care, they can access the group later regardless
        if (performingUser.IsAdmin() is false && userIds.Contains(performingUser.Id) is false)
        {
            userIds.Add(performingUser.Id);
        }

        return userIds;
    }

    /// <inheritdoc />
    public Task<Attempt<UserGroupOperationStatus>> DeleteAsync(IUserGroup userGroup) => throw new NotImplementedException();
}
