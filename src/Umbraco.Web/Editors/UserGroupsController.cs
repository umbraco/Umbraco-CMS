using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
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
            if (userGroupSave == null) throw new ArgumentNullException("userGroupSave");

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

            display.AddSuccessNotification(Services.TextService.Localize("speechBubbles/operationSavedHeader"), Services.TextService.Localize("speechBubbles/editUserGroupSaved"));
            return display;
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
        public IEnumerable<UserGroupBasic> GetUserGroups()
        {
            return Mapper.Map<IEnumerable<IUserGroup>, IEnumerable<UserGroupBasic>>(Services.UserService.GetAllUserGroups());
        }

        /// <summary>
        /// Return a user group
        /// </summary>
        /// <returns></returns>
        public UserGroupDisplay GetUserGroup(int id)
        {
            var found = Services.UserService.GetUserGroupById(id);
            if (found == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            
            var display =  Mapper.Map<UserGroupDisplay>(found);

            return display;
        }
    }
}