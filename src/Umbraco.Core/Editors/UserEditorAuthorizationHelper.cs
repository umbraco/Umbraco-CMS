// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Editors;

public class UserEditorAuthorizationHelper
{
    private readonly AppCaches _appCaches;
    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IMediaService _mediaService;

    public UserEditorAuthorizationHelper(IContentService contentService, IMediaService mediaService, IEntityService entityService, AppCaches appCaches)
    {
        _contentService = contentService;
        _mediaService = mediaService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <summary>
    ///     Checks if the current user has access to save the user data
    /// </summary>
    /// <param name="currentUser">The current user trying to save user data</param>
    /// <param name="savingUser">The user instance being saved (can be null if it's a new user)</param>
    /// <param name="startContentIds">The start content ids of the user being saved (can be null or empty)</param>
    /// <param name="startMediaIds">The start media ids of the user being saved (can be null or empty)</param>
    /// <param name="userGroupAliases">The user aliases of the user being saved (can be null or empty)</param>
    /// <returns></returns>
    public Attempt<string?> IsAuthorized(
        IUser? currentUser,
        IUser? savingUser,
        IEnumerable<int>? startContentIds,
        IEnumerable<int>? startMediaIds,
        IEnumerable<string>? userGroupAliases)
    {
        var currentIsAdmin = currentUser?.IsAdmin() ?? false;

        // a) A non-admin cannot save an admin
        if (savingUser != null)
        {
            if (savingUser.IsAdmin() && currentIsAdmin == false)
            {
                return Attempt.Fail("The current user is not an administrator so cannot save another administrator");
            }
        }

        // b) If a start node is changing, a user cannot set a start node on another user that they don't have access to, this even goes for admins

        // only validate any start nodes that have changed.
        // a user can remove any start nodes and add start nodes that they have access to
        // but they cannot add a start node that they do not have access to
        IEnumerable<int>? changedStartContentIds = savingUser == null
            ? startContentIds
            : startContentIds == null || savingUser.StartContentIds is null
                ? null
                : startContentIds.Except(savingUser.StartContentIds).ToArray();
        IEnumerable<int>? changedStartMediaIds = savingUser == null
            ? startMediaIds
            : startMediaIds == null || savingUser.StartMediaIds is null
                ? null
                : startMediaIds.Except(savingUser.StartMediaIds).ToArray();
        Attempt<string?> pathResult = currentUser is null
            ? Attempt<string?>.Fail()
            : AuthorizePath(currentUser, changedStartContentIds, changedStartMediaIds);
        if (pathResult == false)
        {
            return pathResult;
        }

        // c) an admin can manage any group or section access
        if (currentIsAdmin)
        {
            return Attempt<string?>.Succeed();
        }

        if (userGroupAliases != null)
        {
            var savingGroupAliases = userGroupAliases.ToArray();
            var existingGroupAliases = savingUser == null
                ? new string[0]
                : savingUser.Groups.Select(x => x.Alias).ToArray();

            IEnumerable<string> addedGroupAliases = savingGroupAliases.Except(existingGroupAliases);

            // As we know the current user is not admin, it is only allowed to use groups that the user do have themselves.
            var savingGroupAliasesNotAllowed = addedGroupAliases
                .Except(currentUser?.Groups.Select(x => x.Alias) ?? Enumerable.Empty<string>()).ToArray();
            if (savingGroupAliasesNotAllowed.Any())
            {
                return Attempt.Fail("Cannot assign the group(s) '" + string.Join(", ", savingGroupAliasesNotAllowed) +
                                    "', the current user is not part of them or admin");
            }

            // only validate any groups that have changed.
            // a non-admin user can remove groups and add groups that they have access to
            // but they cannot add a group that they do not have access to or that grants them
            // path or section access that they don't have access to.
            var newGroups = savingUser == null
                ? savingGroupAliases
                : savingGroupAliases.Except(savingUser.Groups.Select(x => x.Alias)).ToArray();

            var userGroupsChanged = savingUser != null && newGroups.Length > 0;

            if (userGroupsChanged)
            {
                // d) A user cannot assign a group to another user that they do not belong to
                var currentUserGroups = currentUser?.Groups.Select(x => x.Alias).ToArray();
                foreach (var group in newGroups)
                {
                    if (currentUserGroups?.Contains(group) == false)
                    {
                        return Attempt.Fail("Cannot assign the group " + group + ", the current user is not a member");
                    }
                }
            }
        }

        return Attempt<string?>.Succeed();
    }

    private Attempt<string?> AuthorizePath(IUser currentUser, IEnumerable<int>? startContentIds, IEnumerable<int>? startMediaIds)
    {
        if (startContentIds != null)
        {
            foreach (var contentId in startContentIds)
            {
                if (contentId == Constants.System.Root)
                {
                    var hasAccess = ContentPermissions.HasPathAccess(
                        "-1",
                        currentUser.CalculateContentStartNodeIds(_entityService, _appCaches),
                        Constants.System.RecycleBinContent);
                    if (hasAccess == false)
                    {
                        return Attempt.Fail("The current user does not have access to the content root");
                    }
                }
                else
                {
                    IContent? content = _contentService.GetById(contentId);
                    if (content == null)
                    {
                        continue;
                    }

                    var hasAccess = currentUser.HasPathAccess(content, _entityService, _appCaches);
                    if (hasAccess == false)
                    {
                        return Attempt.Fail("The current user does not have access to the content path " +
                                            content.Path);
                    }
                }
            }
        }

        if (startMediaIds != null)
        {
            foreach (var mediaId in startMediaIds)
            {
                if (mediaId == Constants.System.Root)
                {
                    var hasAccess = ContentPermissions.HasPathAccess(
                        "-1",
                        currentUser.CalculateMediaStartNodeIds(_entityService, _appCaches),
                        Constants.System.RecycleBinMedia);
                    if (hasAccess == false)
                    {
                        return Attempt.Fail("The current user does not have access to the media root");
                    }
                }
                else
                {
                    IMedia? media = _mediaService.GetById(mediaId);
                    if (media == null)
                    {
                        continue;
                    }

                    var hasAccess = currentUser.HasPathAccess(media, _entityService, _appCaches);
                    if (hasAccess == false)
                    {
                        return Attempt.Fail("The current user does not have access to the media path " + media.Path);
                    }
                }
            }
        }

        return Attempt<string?>.Succeed();
    }
}
