using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class UserGroupPermissionService : IUserGroupPermissionService
{
    private static readonly Task<UserGroupAuthorizationStatus> _successTaskResult = Task.FromResult(UserGroupAuthorizationStatus.Success);
    private static readonly Task<UserGroupAuthorizationStatus> _unauthorizedMissingUserGroupAccessTaskResult = Task.FromResult(UserGroupAuthorizationStatus.UnauthorizedMissingUserGroupAccess);
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IUserGroupRepository _userGroupRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupPermissionService" /> class.
    /// </summary>
    /// <param name="contentService">The content service.</param>
    /// <param name="mediaService">The media service.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="userGroupRepository">The user group repository.</param>
    public UserGroupPermissionService(
        IContentService contentService,
        IMediaService mediaService,
        IEntityService entityService,
        AppCaches appCaches,
        ICoreScopeProvider scopeProvider,
        IUserGroupRepository userGroupRepository)
    {
        _contentService = contentService;
        _mediaService = mediaService;
        _entityService = entityService;
        _appCaches = appCaches;
        _scopeProvider = scopeProvider;
        _userGroupRepository = userGroupRepository;
    }

    /// <inheritdoc/>
    public Task<UserGroupAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> userGroupKeys)
    {
        if (user.IsAdmin())
        {
            return _successTaskResult;
        }

        // LINQ.Except will make a HashSet of the argument passed in, so it is OK to use IEnumerable and avoid an allocation
        var allowedUserGroupsKeys = user.Groups.Select(x => x.Key);
        var missingAccess = userGroupKeys.Except(allowedUserGroupsKeys);

        return missingAccess.Any() is false
            ? _successTaskResult
            : _unauthorizedMissingUserGroupAccessTaskResult;
    }

    /// <inheritdoc/>
    public Task<UserGroupAuthorizationStatus> AuthorizeCreateAsync(IUser user, IUserGroup userGroup)
        => Task.FromResult(ValidateAccess(user, userGroup));


    /// <inheritdoc/>
    public async Task<UserGroupAuthorizationStatus> AuthorizeUpdateAsync(IUser user, IUserGroup userGroup)
    {
        // Authorize that a user belongs to user group
        UserGroupAuthorizationStatus authorizeGroupAccess = await AuthorizeAccessAsync(user, new[] { userGroup.Key });

        return authorizeGroupAccess != UserGroupAuthorizationStatus.Success
            ? authorizeGroupAccess
            : ValidateAccess(user, userGroup);
    }

    /// <summary>
    ///     Validate user's access to create/modify user group.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to validate access.</param>
    /// <param name="userGroup">The user group to be validated.</param>
    /// <returns><see cref="UserGroupAuthorizationStatus"/>.</returns>
    /// <remarks>
    ///     Start nodes of an existing user group are only validated when they differ from the persisted user group, so a
    ///     user can save a user group without being blocked by start nodes they did not change.
    /// </remarks>
    private UserGroupAuthorizationStatus ValidateAccess(IUser user, IUserGroup userGroup)
    {
        var hasAccessToUsersSection = HasAccessToUsersSection(user);
        if (hasAccessToUsersSection is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingUsersSectionAccess;
        }

        // Check that the user is not obtaining more access by specifying sections that they don't have access to.
        var hasAccessToAllGroupSections = HasAccessToAllUserGroupSections(user, userGroup);
        if (hasAccessToAllGroupSections is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingAllowedSectionAccess;
        }

        // Check that the user is not setting start nodes that they don't have access to.
        IUserGroup? persistedUserGroup = userGroup.HasIdentity ? GetPersistedUserGroup(userGroup.Key) : null;

        if (persistedUserGroup?.StartContentId != userGroup.StartContentId
            && HasAccessToContentStartNode(user, userGroup) is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess;
        }

        if (persistedUserGroup?.StartMediaId != userGroup.StartMediaId
            && HasAccessToMediaStartNode(user, userGroup) is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess;
        }

        if (persistedUserGroup?.StartElementId != userGroup.StartElementId
            && HasAccessToElementStartNode(user, userGroup) is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingElementStartNodeAccess;
        }

        return UserGroupAuthorizationStatus.Success;
    }

    /// <summary>
    ///     Gets the persisted state of a user group.
    /// </summary>
    /// <param name="key">The key of the user group.</param>
    /// <returns>The persisted user group, or <c>null</c> if it does not exist.</returns>
    private IUserGroup? GetPersistedUserGroup(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IQuery<IUserGroup> query = _scopeProvider.CreateQuery<IUserGroup>().Where(x => x.Key == key);
        return _userGroupRepository.Get(query).FirstOrDefault();
    }

    /// <summary>
    ///     Check that a user has access to the Users section.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private static bool HasAccessToUsersSection(IUser user)
        => user.AllowedSections.Contains(Constants.Applications.Users);

    /// <summary>
    ///     Check that a user is not adding a section to the group that they don't have access to.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="userGroup">The user group being created or updated.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private static bool HasAccessToAllUserGroupSections(IUser user, IUserGroup userGroup)
    {
        if (user.IsAdmin())
        {
            return true;
        }

        var sectionsMissingAccess = userGroup.AllowedSections.Except(user.AllowedSections);
        return sectionsMissingAccess.Any() is false;
    }

    /// <summary>
    ///     Check that a user has access to the content start node.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="userGroup">The user group being created or updated.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private bool HasAccessToContentStartNode(IUser user, IUserGroup userGroup)
    {
        if (userGroup.StartContentId is null)
        {
            return true;
        }

        if (userGroup.StartContentId == Constants.System.Root)
        {
            return user.HasContentRootAccess(_entityService, _appCaches);
        }

        IContent? content = _contentService.GetById(userGroup.StartContentId.Value);

        if (content is null)
        {
            return true;
        }

        return user.HasPathAccess(content, _entityService, _appCaches);
    }

    /// <summary>
    ///     Check that a user has access to the media start node.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="userGroup">The user group being created or updated.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private bool HasAccessToMediaStartNode(IUser user, IUserGroup userGroup)
    {
        if (userGroup.StartMediaId is null)
        {
            return true;
        }

        if (userGroup.StartMediaId == Constants.System.Root)
        {
            return user.HasMediaRootAccess(_entityService, _appCaches);
        }

        IMedia? media = _mediaService.GetById(userGroup.StartMediaId.Value);

        if (media is null)
        {
            return true;
        }

        return user.HasPathAccess(media, _entityService, _appCaches);
    }

    /// <summary>
    ///     Check that a user has access to the element start node.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="userGroup">The user group being created or updated.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private bool HasAccessToElementStartNode(IUser user, IUserGroup userGroup)
    {
        if (userGroup.StartElementId is null)
        {
            return true;
        }

        if (userGroup.StartElementId == Constants.System.Root)
        {
            return user.HasElementRootAccess(_entityService, _appCaches);
        }

        IEntitySlim? container = _entityService.Get(userGroup.StartElementId.Value, UmbracoObjectTypes.ElementContainer);

        if (container is null)
        {
            return true;
        }

        return user.HasElementPathAccess(container, _entityService, _appCaches);
    }
}
