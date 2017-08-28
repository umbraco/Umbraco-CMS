using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Web.Editors
{
    internal class UserEditorAuthorizationHelper
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;

        public UserEditorAuthorizationHelper(IContentService contentService, IMediaService mediaService, IUserService userService, IEntityService entityService)
        {
            _contentService = contentService;
            _mediaService = mediaService;
            _userService = userService;
            _entityService = entityService;
        }

        /// <summary>
        /// Checks if the current user has access to save the user data 
        /// </summary>
        /// <param name="currentUser">The current user trying to save user data</param>
        /// <param name="savingUser">The user instance being saved (can be null if it's a new user)</param>
        /// <param name="startContentIds">The start content ids of the user being saved (can be null or empty)</param>
        /// <param name="startMediaIds">The start media ids of the user being saved (can be null or empty)</param>
        /// <param name="userGroupAliases">The user aliases of the user being saved (can be null or empty)</param>
        /// <returns></returns>
        public Attempt<string> IsAuthorized(IUser currentUser,
            IUser savingUser,
            IEnumerable<int> startContentIds, IEnumerable<int> startMediaIds,
            IEnumerable<string> userGroupAliases)
        {
            var currentIsAdmin = currentUser.IsAdmin();

            // a) A non-admin cannot save an admin

            if (savingUser != null)
            {                
                if (savingUser.IsAdmin() && currentIsAdmin == false)
                    return Attempt.Fail("The current user is not an administrator so cannot save another administrator");
            }

            // b) A user cannot set a start node on another user that they don't have access to, this even goes for admins

            var pathResult = AuthorizePath(currentUser, startContentIds, startMediaIds);
            if (pathResult == false)
                return pathResult;
            
            // c) an admin can manage any group or section access
            
            if (currentIsAdmin)
                return Attempt<string>.Succeed();

            if (userGroupAliases != null)
            {
                var savingGroupAliases = userGroupAliases.ToArray();

                //only validate any groups that have changed.
                //a non-admin user can remove groups and add groups that they have access to
                //but they cannot add a group that they do not have access to or that grants them
                //path or section access that they don't have access to.

                var newGroups = savingUser == null
                    ? savingGroupAliases
                    : savingGroupAliases.Except(savingUser.Groups.Select(x => x.Alias)).ToArray();

                var userGroupsChanged = savingUser != null && newGroups.Length > 0;

                if (userGroupsChanged)
                {
                    var userGroups = _userService.GetUserGroupsByAlias(newGroups).ToArray();

                    //TODO: pretty sure d + e can be done by just checking if the newGroups being added are groups that the current user is a member of

                    // d) A user cannot assign a group to another user that grants them access to a start node they don't have access to
                    foreach (var group in userGroups)
                    {
                        pathResult = AuthorizePath(currentUser,
                            group.StartContentId.HasValue ? new[] { group.StartContentId.Value } : null,
                            group.StartMediaId.HasValue ? new[] { group.StartMediaId.Value } : null);
                        if (pathResult == false)
                            return pathResult;
                    }

                    // e) A user cannot set a section on another user that they don't have access to
                    var allGroupSections = userGroups.SelectMany(x => x.AllowedSections).Distinct();
                    var missingSectionAccess = allGroupSections.Except(currentUser.AllowedSections).ToArray();
                    if (missingSectionAccess.Length > 0)
                    {
                        return Attempt.Fail("The current user does not have access to sections " + string.Join(",", missingSectionAccess));
                    }
                }
            }            

            return Attempt<string>.Succeed();
        }

        private Attempt<string> AuthorizePath(IUser currentUser, IEnumerable<int> startContentIds, IEnumerable<int> startMediaIds)
        {
            if (startContentIds != null)
            {
                foreach (var contentId in startContentIds)
                {
                    var content = _contentService.GetById(contentId);
                    if (content == null) continue;
                    var hasAccess = currentUser.HasPathAccess(content, _entityService);
                    if (hasAccess == false)
                        return Attempt.Fail("The current user does not have access to the content path " + content.Path);
                }
            }

            if (startMediaIds != null)
            {
                foreach (var mediaId in startMediaIds)
                {
                    var media = _mediaService.GetById(mediaId);
                    if (media == null) continue;
                    var hasAccess = currentUser.HasPathAccess(media, _entityService);
                    if (hasAccess == false)
                        return Attempt.Fail("The current user does not have access to the media path " + media.Path);
                }                
            }

            return Attempt<string>.Succeed();
        }
    }
}