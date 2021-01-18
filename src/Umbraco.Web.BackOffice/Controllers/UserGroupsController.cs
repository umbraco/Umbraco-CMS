using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.BackOffice.ActionResults;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Authorization;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Models.ContentEditing;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessUsers)]
    [PrefixlessBodyModelValidator]
    public class UserGroupsController : UmbracoAuthorizedJsonController
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly IEntityService _entityService;
        private readonly IMediaService _mediaService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IShortStringHelper _shortStringHelper;

        public UserGroupsController(IUserService userService, IContentService contentService,
            IEntityService entityService, IMediaService mediaService, IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            UmbracoMapper umbracoMapper, ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _localizedTextService =
                localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        }

        [UserGroupValidate]
        public ActionResult<UserGroupDisplay> PostSaveUserGroup(UserGroupSave userGroupSave)
        {
            if (userGroupSave == null) throw new ArgumentNullException(nameof(userGroupSave));

            //authorize that the user has access to save this user group
            var authHelper = new UserGroupEditorAuthorizationHelper(
                _userService, _contentService, _mediaService, _entityService);

            var isAuthorized = authHelper.AuthorizeGroupAccess(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, userGroupSave.Alias);
            if (isAuthorized == false)
                return Unauthorized(isAuthorized.Result);

            //if sections were added we need to check that the current user has access to that section
            isAuthorized = authHelper.AuthorizeSectionChanges(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                userGroupSave.PersistedUserGroup.AllowedSections,
                userGroupSave.Sections);
            if (isAuthorized == false)
                return Unauthorized(isAuthorized.Result);

            //if start nodes were changed we need to check that the current user has access to them
            isAuthorized = authHelper.AuthorizeStartNodeChanges(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                userGroupSave.PersistedUserGroup.StartContentId,
                userGroupSave.StartContentId,
                userGroupSave.PersistedUserGroup.StartMediaId,
                userGroupSave.StartMediaId);
            if (isAuthorized == false)
                return Unauthorized(isAuthorized.Result);

            //need to ensure current user is in a group if not an admin to avoid a 401
            EnsureNonAdminUserIsInSavedUserGroup(userGroupSave);

            //save the group
            _userService.Save(userGroupSave.PersistedUserGroup, userGroupSave.Users.ToArray());

            //deal with permissions

            //remove ones that have been removed
            var existing = _userService.GetPermissions(userGroupSave.PersistedUserGroup, true)
                .ToDictionary(x => x.EntityId, x => x);
            var toRemove = existing.Keys.Except(userGroupSave.AssignedPermissions.Select(x => x.Key));
            foreach (var contentId in toRemove)
            {
                _userService.RemoveUserGroupPermissions(userGroupSave.PersistedUserGroup.Id, contentId);
            }

            //update existing
            foreach (var assignedPermission in userGroupSave.AssignedPermissions)
            {
                _userService.ReplaceUserGroupPermissions(
                    userGroupSave.PersistedUserGroup.Id,
                    assignedPermission.Value.Select(x => x[0]),
                    assignedPermission.Key);
            }

            var display = _umbracoMapper.Map<UserGroupDisplay>(userGroupSave.PersistedUserGroup);

            display.AddSuccessNotification(_localizedTextService.Localize("speechBubbles/operationSavedHeader"), _localizedTextService.Localize("speechBubbles/editUserGroupSaved"));
            return display;
        }

        private void EnsureNonAdminUserIsInSavedUserGroup(UserGroupSave userGroupSave)
        {
            if (_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin())
            {
                return;
            }

            var userIds = userGroupSave.Users.ToList();
            if (userIds.Contains(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id))
            {
                return;
            }

            userIds.Add(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
            userGroupSave.Users = userIds;
        }

        /// <summary>
        /// Returns the scaffold for creating a new user group
        /// </summary>
        /// <returns></returns>
        public UserGroupDisplay GetEmptyUserGroup()
        {
            return _umbracoMapper.Map<UserGroupDisplay>(new UserGroup(_shortStringHelper));
        }

        /// <summary>
        /// Returns all user groups
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserGroupBasic> GetUserGroups(bool onlyCurrentUserGroups = true)
        {
            var allGroups = _umbracoMapper.MapEnumerable<IUserGroup, UserGroupBasic>(_userService.GetAllUserGroups())
                .ToList();

            var isAdmin = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.IsAdmin();
            if (isAdmin) return allGroups;

            if (onlyCurrentUserGroups == false)
            {
                //this user is not an admin so in that case we need to exclude all admin users
                allGroups.RemoveAt(allGroups.IndexOf(allGroups.Find(basic => basic.Alias == Constants.Security.AdminGroupAlias)));
                return allGroups;
            }

            //we cannot return user groups that this user does not have access to
            var currentUserGroups = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Groups.Select(x => x.Alias).ToArray();
            return allGroups.Where(x => currentUserGroups.Contains(x.Alias)).ToArray();
        }

        /// <summary>
        /// Return a user group
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = AuthorizationPolicies.UserBelongsToUserGroupInRequest)]
        public ActionResult<UserGroupDisplay> GetUserGroup(int id)
        {
            var found = _userService.GetUserGroupById(id);
            if (found == null)
                return NotFound();

            var display =  _umbracoMapper.Map<UserGroupDisplay>(found);

            return display;
        }

        [HttpPost]
        [HttpDelete]
        [Authorize(Policy = AuthorizationPolicies.UserBelongsToUserGroupInRequest)]
        public IActionResult PostDeleteUserGroups([FromQuery] int[] userGroupIds)
        {
            var userGroups = _userService.GetAllUserGroups(userGroupIds)
                //never delete the admin group, sensitive data or translators group
                .Where(x => !x.IsSystemUserGroup())
                .ToArray();
            foreach (var userGroup in userGroups)
            {
                _userService.DeleteUserGroup(userGroup);
            }
            if (userGroups.Length > 1)
                return new UmbracoNotificationSuccessResponse(
                    _localizedTextService.Localize("speechBubbles/deleteUserGroupsSuccess", new[] {userGroups.Length.ToString()}));
            return new UmbracoNotificationSuccessResponse(
                _localizedTextService.Localize("speechBubbles/deleteUserGroupSuccess", new[] {userGroups[0].Name}));
        }
    }
}
