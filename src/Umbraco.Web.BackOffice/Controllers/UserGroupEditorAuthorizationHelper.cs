using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

internal class UserGroupEditorAuthorizationHelper
{
    private readonly AppCaches _appCaches;
    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IMediaService _mediaService;
    private readonly IUserService _userService;

    public UserGroupEditorAuthorizationHelper(IUserService userService, IContentService contentService, IMediaService mediaService, IEntityService entityService, AppCaches appCaches)
    {
        _userService = userService;
        _contentService = contentService;
        _mediaService = mediaService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <summary>
    ///     Authorize that the current user belongs to these groups
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="groupIds"></param>
    /// <returns></returns>
    public Attempt<string?> AuthorizeGroupAccess(IUser? currentUser, params int[] groupIds)
    {
        if (currentUser?.IsAdmin() ?? false)
        {
            return Attempt<string?>.Succeed();
        }

        IEnumerable<IUserGroup> groups = _userService.GetAllUserGroups(groupIds.ToArray());
        var groupAliases = groups.Select(x => x.Alias).ToArray();
        var userGroups = currentUser?.Groups.Select(x => x.Alias).ToArray() ?? Array.Empty<string>();
        var missingAccess = groupAliases.Except(userGroups).ToArray();
        return missingAccess.Length == 0
            ? Attempt<string?>.Succeed()
            : Attempt.Fail("User is not a member of " + string.Join(", ", missingAccess));
    }

    /// <summary>
    ///     Authorize that the current user belongs to these groups
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="groupAliases"></param>
    /// <returns></returns>
    public Attempt<string?> AuthorizeGroupAccess(IUser? currentUser, params string[] groupAliases)
    {
        if (currentUser?.IsAdmin() ?? false)
        {
            return Attempt<string?>.Succeed();
        }

        IEnumerable<IUserGroup> existingGroups = _userService.GetUserGroupsByAlias(groupAliases);

        if (!existingGroups.Any())
        {
            // We're dealing with new groups,
            // so authorization should be given to any user with access to Users section
            if (currentUser?.AllowedSections.Contains(Constants.Applications.Users) ?? false)
            {
                return Attempt<string?>.Succeed();
            }
        }

        var userGroups = currentUser?.Groups.Select(x => x.Alias).ToArray();
        var missingAccess = groupAliases.Except(userGroups ?? Array.Empty<string>()).ToArray();
        return missingAccess.Length == 0
            ? Attempt<string?>.Succeed()
            : Attempt.Fail("User is not a member of " + string.Join(", ", missingAccess));
    }

    /// <summary>
    ///     Authorize that the user is not adding a section to the group that they don't have access to
    /// </summary>
    public Attempt<string?> AuthorizeSectionChanges(
        IUser? currentUser,
        IEnumerable<string>? existingSections,
        IEnumerable<string>? proposedAllowedSections)
    {
        if (currentUser?.IsAdmin() ?? false)
        {
            return Attempt<string?>.Succeed();
        }

        var sectionsAdded = proposedAllowedSections?.Except(existingSections ?? Enumerable.Empty<string>()).ToArray();
        var sectionAccessMissing =
            sectionsAdded?.Except(currentUser?.AllowedSections ?? Enumerable.Empty<string>()).ToArray();
        return sectionAccessMissing?.Length > 0
            ? Attempt.Fail("Current user doesn't have access to add these sections " +
                           string.Join(", ", sectionAccessMissing))
            : Attempt<string?>.Succeed();
    }

    /// <summary>
    ///     Authorize that the user is not changing to a start node that they don't have access to (including admins)
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="currentContentStartId"></param>
    /// <param name="proposedContentStartId"></param>
    /// <param name="currentMediaStartId"></param>
    /// <param name="proposedMediaStartId"></param>
    /// <returns></returns>
    public Attempt<string?> AuthorizeStartNodeChanges(
        IUser? currentUser,
        int? currentContentStartId,
        int? proposedContentStartId,
        int? currentMediaStartId,
        int? proposedMediaStartId)
    {
        if (currentContentStartId != proposedContentStartId && proposedContentStartId.HasValue)
        {
            IContent? content = _contentService.GetById(proposedContentStartId.Value);
            if (content != null)
            {
                if (currentUser?.HasPathAccess(content, _entityService, _appCaches) == false)
                {
                    return Attempt.Fail("Current user doesn't have access to the content path " + content.Path);
                }
            }
        }

        if (currentMediaStartId != proposedMediaStartId && proposedMediaStartId.HasValue)
        {
            IMedia? media = _mediaService.GetById(proposedMediaStartId.Value);
            if (media != null)
            {
                if (currentUser?.HasPathAccess(media, _entityService, _appCaches) == false)
                {
                    return Attempt.Fail("Current user doesn't have access to the media path " + media.Path);
                }
            }
        }

        return Attempt<string?>.Succeed();
    }
}
