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

            Services.UserService.Save(userGroupSave.PersistedUserGroup, userGroupSave.Users.ToArray());

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

            var allContentPermissions = Services.UserService.GetPermissions(found, true)
                .ToDictionary(x => x.EntityId, x => x);

            var display =  Mapper.Map<UserGroupDisplay>(found);

            var contentEntities = Services.EntityService.GetAll(UmbracoObjectTypes.Document, allContentPermissions.Keys.ToArray());
            var allAssignedPermissions = new List<AssignedContentPermissions>();
            foreach (var entity in contentEntities)
            {
                var contentPermissions = allContentPermissions[entity.Id];

                var assignedContentPermissions = Mapper.Map<AssignedContentPermissions>(entity);
                assignedContentPermissions.AssignedPermissions = AssignedUserGroupPermissions.ClonePermissions(display.DefaultPermissions);

                //since there is custom permissions assigned to this node for this group, we need to clear all of the default permissions
                //and we'll re-check it if it's one of the explicitly assigned ones
                foreach (var permission in assignedContentPermissions.AssignedPermissions.SelectMany(x => x.Value))
                {
                    permission.Checked = false;
                    permission.Checked = contentPermissions.AssignedPermissions.Contains(permission.PermissionCode, StringComparer.InvariantCulture);
                }

                allAssignedPermissions.Add(assignedContentPermissions);
            }

            display.AssignedPermissions = allAssignedPermissions;

            return display;
        }
    }
}