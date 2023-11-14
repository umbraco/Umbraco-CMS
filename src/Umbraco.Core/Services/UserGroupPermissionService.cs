using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class UserGroupPermissionService : IUserGroupPermissionService
{
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public UserGroupPermissionService(
        IContentService contentService,
        IMediaService mediaService,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _contentService = contentService;
        _mediaService = mediaService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public async Task<UserGroupAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> userGroupKeys)
    {
        if (performingUser.IsAdmin())
        {
            return UserGroupAuthorizationStatus.Success;
        }

        var allowedUserGroupsKeys = performingUser.Groups.Select(x => x.Key).ToArray();
        var missingAccess = userGroupKeys.Except(allowedUserGroupsKeys).ToArray();

        return missingAccess.Length == 0
            ? UserGroupAuthorizationStatus.Success
            : UserGroupAuthorizationStatus.UnauthorizedMissingUserGroupAccess;
    }

    /// <inheritdoc/>
    public async Task<UserGroupAuthorizationStatus> AuthorizeCreateAsync(IUser performingUser, IUserGroup userGroup)
    {
        var hasAccessToUserSection = HasAccessToUserSection(performingUser);
        if (hasAccessToUserSection is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingUserSectionAccess;
        }

        var hasAccessToAllGroupSections = HasAccessToAllUserGroupSections(performingUser, userGroup);
        if (hasAccessToAllGroupSections is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingAllowedSectionAccess;
        }

        // Check that the user is not setting start nodes that they don't have access to.
        var hasContentStartNodeAccess = HasAccessToContentStartNode(performingUser, userGroup);
        if (hasContentStartNodeAccess is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess;
        }

        var hasMediaStartNodeAccess = HasAccessToMediaStartNode(performingUser, userGroup);
        if (hasMediaStartNodeAccess is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess;
        }

        return UserGroupAuthorizationStatus.Success;
    }

    /// <inheritdoc/>
    public async Task<UserGroupAuthorizationStatus> AuthorizeUpdateAsync(IUser performingUser, IUserGroup userGroup)
    {
        var hasAccessToUserSection = HasAccessToUserSection(performingUser);
        if (hasAccessToUserSection is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingUserSectionAccess;
        }

        UserGroupAuthorizationStatus authorizeGroupAccess = await AuthorizeAccessAsync(performingUser, new[] { userGroup.Key });
        if (authorizeGroupAccess != UserGroupAuthorizationStatus.Success)
        {
            return authorizeGroupAccess;
        }

        var hasAccessToAllGroupSections = HasAccessToAllUserGroupSections(performingUser, userGroup);
        if (hasAccessToAllGroupSections is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingAllowedSectionAccess;
        }

        // Check that the user is not setting start nodes that they don't have access to.
        var hasContentStartNodeAccess = HasAccessToContentStartNode(performingUser, userGroup);
        if (hasContentStartNodeAccess is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess;
        }

        var hasMediaStartNodeAccess = HasAccessToMediaStartNode(performingUser, userGroup);
        if (hasMediaStartNodeAccess is false)
        {
            return UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess;
        }

        return UserGroupAuthorizationStatus.Success;
    }

    /// <summary>
    ///     Check that the user has access to the user section.
    /// </summary>
    /// <param name="user">The user performing the action.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private bool HasAccessToUserSection(IUser user)
        => user.AllowedSections.Contains(Constants.Applications.Users);

    /// <summary>
    ///     Check that the user is not adding a section to the group that they don't have access to.
    /// </summary>
    /// <param name="performingUser">The user performing the action.</param>
    /// <param name="userGroup">The user group being created or updated.</param>
    /// <returns>An attempt with an operation status.</returns>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private bool HasAccessToAllUserGroupSections(IUser performingUser, IUserGroup userGroup)
    {
        if (performingUser.IsAdmin())
        {
            return true;
        }

        var sectionsMissingAccess = userGroup.AllowedSections.Except(performingUser.AllowedSections).ToArray();
        return sectionsMissingAccess.Length == 0;
    }

    /// <summary>
    ///     Check that the user has access to the content start node.
    /// </summary>
    /// <param name="user">The user performing the action.</param>
    /// <param name="userGroup">The user group being created or updated.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private bool HasAccessToContentStartNode(IUser user, IUserGroup userGroup)
    {
        if (userGroup.StartContentId is null)
        {
            return true;
        }

        IContent? content = _contentService.GetById(userGroup.StartContentId.Value);

        if (content is null)
        {
            return true;
        }

        return user.HasPathAccess(content, _entityService, _appCaches);
    }

    /// <summary>
    ///     Check that the user has access to the media start node.
    /// </summary>
    /// <param name="user">The user performing the action.</param>
    /// <param name="userGroup">The user group being created or updated.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
    private bool HasAccessToMediaStartNode(IUser user, IUserGroup userGroup)
    {
        if (userGroup.StartMediaId is null)
        {
            return true;
        }

        IMedia? media = _mediaService.GetById(userGroup.StartMediaId.Value);

        if (media is null)
        {
            return true;
        }

        return user.HasPathAccess(media, _entityService, _appCaches);
    }
}
