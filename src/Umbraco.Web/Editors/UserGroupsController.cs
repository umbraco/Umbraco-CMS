using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors.Filters;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Users)]
    [PrefixlessBodyModelValidator]
    public class UserGroupsController : UmbracoAuthorizedJsonController
    {
        [UserGroupValidate]
        public UserGroupDisplay PostSaveUserGroup(UserGroupSave userGroupSave)
        {
            if (userGroupSave == null) throw new ArgumentNullException(nameof(userGroupSave));

            //authorize that the user has access to save this user group
            var authHelper = new UserGroupEditorAuthorizationHelper(
                Services.UserService,
                Services.ContentService,
                Services.MediaService,
                Services.EntityService,
                AppCaches);

            var isAuthorized = authHelper.AuthorizeGroupAccess(Security.CurrentUser, userGroupSave.Alias);
            if (isAuthorized == false)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized, isAuthorized.Result));

            //if sections were added we need to check that the current user has access to that section
            isAuthorized = authHelper.AuthorizeSectionChanges(
                Security.CurrentUser,
                userGroupSave.PersistedUserGroup.AllowedSections,
                userGroupSave.Sections);
            if (isAuthorized == false)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized, isAuthorized.Result));

            //if start nodes were changed we need to check that the current user has access to them
            isAuthorized = authHelper.AuthorizeStartNodeChanges(Security.CurrentUser,
                userGroupSave.PersistedUserGroup.StartContentId,
                userGroupSave.StartContentId,
                userGroupSave.PersistedUserGroup.StartMediaId,
                userGroupSave.StartMediaId);
            if (isAuthorized == false)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized, isAuthorized.Result));

            //need to ensure current user is in a group if not an admin to avoid a 401
            EnsureNonAdminUserIsInSavedUserGroup(userGroupSave);

            //map the model to the persisted instance
            Mapper.Map(userGroupSave, userGroupSave.PersistedUserGroup);

            //save the group
            Services.UserService.Save(userGroupSave.PersistedUserGroup, userGroupSave.Users.ToArray());

            //deal with permissions

            //remove ones that have been removed
            var existing = Services.UserService.GetPermissions(userGroupSave.PersistedUserGroup, true)
                .ToDictionary(x => x.EntityId, x => x);
            var toRemove = existing.Keys.Except(userGroupSave.AssignedPermissions.Select(x => x.Key));
            foreach (var contentId in toRemove)
            {
                Services.UserService.RemoveUserGroupPermissions(userGroupSave.PersistedUserGroup.Id, contentId);
            }

            //update existing
            foreach (var assignedPermission in userGroupSave.AssignedPermissions)
            {
                Services.UserService.ReplaceUserGroupPermissions(
                    userGroupSave.PersistedUserGroup.Id,
                    assignedPermission.Value.Select(x => x[0]),
                    assignedPermission.Key);
            }

            var display = Mapper.Map<UserGroupDisplay>(userGroupSave.PersistedUserGroup);

            display.AddSuccessNotification(Services.TextService.Localize("speechBubbles", "operationSavedHeader"), Services.TextService.Localize("speechBubbles", "editUserGroupSaved"));
            return display;
        }

        private void EnsureNonAdminUserIsInSavedUserGroup(UserGroupSave userGroupSave)
        {
            if (Security.CurrentUser.IsAdmin())
            {
                return;
            }

            var userIds = userGroupSave.Users.ToList();
            if (userIds.Contains(Security.CurrentUser.Id))
            {
                return;
            }

            userIds.Add(Security.CurrentUser.Id);
            userGroupSave.Users = userIds;
        }

        /// <summary>
        /// Returns the scaffold for creating a new user group
        /// </summary>
        /// <returns></returns>
        public UserGroupDisplay GetEmptyUserGroup()
        {
            return Mapper.Map<UserGroupDisplay>(new UserGroup());
        }

        /// <summary>
        /// Returns all user groups
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserGroupBasic> GetUserGroups(bool onlyCurrentUserGroups = true)
        {
            var allGroups = Mapper.MapEnumerable<IUserGroup, UserGroupBasic>(Services.UserService.GetAllUserGroups())
                .ToList();

            var isAdmin = Security.CurrentUser.IsAdmin();
            if (isAdmin) return allGroups;

            if (onlyCurrentUserGroups == false)
            {
                //this user is not an admin so in that case we need to exclude all admin users
                allGroups.RemoveAt(allGroups.IndexOf(allGroups.Find(basic => basic.Alias == Constants.Security.AdminGroupAlias)));
                return allGroups;
            }

            //we cannot return user groups that this user does not have access to
            var currentUserGroups = Security.CurrentUser.Groups.Select(x => x.Alias).ToArray();
            return allGroups.Where(x => currentUserGroups.Contains(x.Alias)).ToArray();
        }

        /// <summary>
        /// Return a user group
        /// </summary>
        /// <returns></returns>
        [UserGroupAuthorization("id")]
        public UserGroupDisplay GetUserGroup(int id)
        {
            var found = Services.UserService.GetUserGroupById(id);
            if (found == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var display =  Mapper.Map<UserGroupDisplay>(found);

            return display;
        }

        [HttpPost]
        [HttpDelete]
        [UserGroupAuthorization("userGroupIds")]
        public HttpResponseMessage PostDeleteUserGroups([FromUri] int[] userGroupIds)
        {
            var userGroups = Services.UserService.GetAllUserGroups(userGroupIds)
                //never delete the admin group, sensitive data or translators group
                .Where(x => !x.IsSystemUserGroup())
                .ToArray();
            foreach (var userGroup in userGroups)
            {
                Services.UserService.DeleteUserGroup(userGroup);
            }
            if (userGroups.Length > 1)
                return Request.CreateNotificationSuccessResponse(
                    Services.TextService.Localize("speechBubbles", "deleteUserGroupsSuccess", new[] {userGroups.Length.ToString()}));
            return Request.CreateNotificationSuccessResponse(
                Services.TextService.Localize("speechBubbles", "deleteUserGroupSuccess", new[] {userGroups[0].Name}));
        }
    }
}
