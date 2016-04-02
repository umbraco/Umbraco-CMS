using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web._Legacy.Actions;
using Action = Umbraco.Web._Legacy.Actions.Action;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Services;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An API controller to deal with user types.
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.UserTypes)]    
    public class UserTypeController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UserTypeController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public UserTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        public UserTypeDisplay GetById(int id)
        {
            var ut = Services.UserService.GetUserTypeById(id);
            if (ut == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var dto = Mapper.Map<IUserType, UserTypeDisplay>(ut);            
            return dto;
        }

        public IEnumerable<PermissionDisplay> GetPermissions()
        {
            var permissions = new List<PermissionDisplay>();
            // There must be a better way!
            var permissionsActions = Action.GetPermissionAssignable();
            foreach (var per in permissionsActions)
            {
                permissions.Add(new PermissionDisplay() {Key = per.Letter.ToString(), Name = per.Alias});
            }
            return permissions;
        }


        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var ut = Services.UserService.GetUserTypeById(id);
            if (ut == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.UserService.DeleteUserType(ut);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        
        public UserTypeDisplay GetEmpty()
        {
            var item = new UserType();
            return Mapper.Map<IUserType, UserTypeDisplay>(item);
        }

        public UserTypeDisplay PostSave(UserTypeSave saveModel)
        {

            var id = saveModel.Id;
            var userType = id > 0 ? 
                ApplicationContext.Services.UserService.GetUserTypeById(id) : 
                new UserType(saveModel.Name, saveModel.Name);
            if (userType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            userType.Name = saveModel.Name;
            userType.Permissions = saveModel.Permissions;
            ApplicationContext.Services.UserService.SaveUserType(userType);

            var display = Mapper.Map<IUserType, UserTypeDisplay>(userType);
            display.AddSuccessNotification(Services.TextService.Localize("speechBubbles/editUserTypeSaved"), string.Empty);
            return display;
        }
    }
}
