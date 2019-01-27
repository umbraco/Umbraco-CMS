using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Web.Editors
{
    internal class UserGroupEditorAuthorizationHelper
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;

        public UserGroupEditorAuthorizationHelper(IUserService userService, IContentService contentService, IMediaService mediaService, IEntityService entityService)
        {
            _userService = userService;
            _contentService = contentService;
            _mediaService = mediaService;
            _entityService = entityService;
        }

        /// <summary>
        /// Authorize that the current user belongs to these groups
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="groupIds"></param>
        /// <returns></returns>
        public Attempt<string> AuthorizeGroupAccess(IUser currentUser, params int[] groupIds)
        {
            if (currentUser.IsAdmin())
                return Attempt<string>.Succeed();

            var groups = _userService.GetAllUserGroups(groupIds.ToArray());
            var groupAliases = groups.Select(x => x.Alias).ToArray();
            var userGroups = currentUser.Groups.Select(x => x.Alias).ToArray();
            var missingAccess = groupAliases.Except(userGroups).ToArray();
            return missingAccess.Length == 0
                ? Attempt<string>.Succeed()
                : Attempt.Fail("User is not a member of " + string.Join(", ", missingAccess));
        }

        /// <summary>
        /// Authorize that the current user belongs to these groups
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="groupAliases"></param>
        /// <returns></returns>
        public Attempt<string> AuthorizeGroupAccess(IUser currentUser, params string[] groupAliases)
        {
            if (currentUser.IsAdmin())
                return Attempt<string>.Succeed();

            var existingGroups = _userService.GetUserGroupsByAlias(groupAliases);

            if(!existingGroups.Any())
            {
                // We're dealing with new groups,
                // so authorization should be given to any user with access to Users section
                if (currentUser.AllowedSections.Contains(Constants.Applications.Users))
                    return Attempt<string>.Succeed();
            }

            var userGroups = currentUser.Groups.Select(x => x.Alias).ToArray();
            var missingAccess = groupAliases.Except(userGroups).ToArray();
            return missingAccess.Length == 0
                ? Attempt<string>.Succeed()
                : Attempt.Fail("User is not a member of " + string.Join(", ", missingAccess));
        }

        /// <summary>
        /// Authorize that the user is not adding a section to the group that they don't have access to
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="currentAllowedSections"></param>
        /// <param name="proposedAllowedSections"></param>
        /// <returns></returns>
        public Attempt<string> AuthorizeSectionChanges(IUser currentUser,
            IEnumerable<string> currentAllowedSections,
            IEnumerable<string> proposedAllowedSections)
        {
            if (currentUser.IsAdmin())
                return Attempt<string>.Succeed();

            var sectionsAdded = currentAllowedSections.Except(proposedAllowedSections).ToArray();
            var sectionAccessMissing = sectionsAdded.Except(currentUser.AllowedSections).ToArray();
            return sectionAccessMissing.Length > 0
                ? Attempt.Fail("Current user doesn't have access to add these sections " + string.Join(", ", sectionAccessMissing))
                : Attempt<string>.Succeed();
        }

        /// <summary>
        /// Authorize that the user is not changing to a start node that they don't have access to (including admins)
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="currentContentStartId"></param>
        /// <param name="proposedContentStartId"></param>
        /// <param name="currentMediaStartId"></param>
        /// <param name="proposedMediaStartId"></param>
        /// <returns></returns>
        public Attempt<string> AuthorizeStartNodeChanges(IUser currentUser,
            int? currentContentStartId,
            int? proposedContentStartId,
            int? currentMediaStartId,
            int? proposedMediaStartId)
        {
            if (currentContentStartId != proposedContentStartId && proposedContentStartId.HasValue)
            {
                var content = _contentService.GetById(proposedContentStartId.Value);
                if (content != null)
                {
                    if (currentUser.HasPathAccess(content, _entityService) == false)
                        return Attempt.Fail("Current user doesn't have access to the content path " + content.Path);
                }
            }

            if (currentMediaStartId != proposedMediaStartId && proposedMediaStartId.HasValue)
            {
                var media = _mediaService.GetById(proposedMediaStartId.Value);
                if (media != null)
                {
                    if (currentUser.HasPathAccess(media, _entityService) == false)
                        return Attempt.Fail("Current user doesn't have access to the media path " + media.Path);
                }
            }

            return Attempt<string>.Succeed();
        }
    }
}
