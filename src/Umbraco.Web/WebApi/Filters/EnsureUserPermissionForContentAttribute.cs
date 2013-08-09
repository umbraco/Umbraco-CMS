using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Auth filter to check if the current user has access to the content item (by id). 
    /// </summary>
    /// <remarks>
    /// 
    /// This first checks if the user can access this based on their start node, and then checks node permissions
    /// 
    /// By default the permission that is checked is browse but this can be specified in the ctor.
    /// NOTE: This cannot be an auth filter because that happens too soon and we don't have access to the action params.
    /// </remarks>
    internal sealed class EnsureUserPermissionForContentAttribute : ActionFilterAttribute
    {
        private int? _nodeId;
        private readonly IUser _user;
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private IContentService ContentService
        {
            get { return _contentService ?? ApplicationContext.Current.Services.ContentService; }
        }
        private IUserService UserService
        {
            get { return _userService ?? ApplicationContext.Current.Services.UserService; }
        }
        private IUser User
        {
            get { return _user ?? UmbracoContext.Current.Security.CurrentUser; }
        }

        private readonly string _paramName;
        private readonly char _permissionToCheck;

        /// <summary>
        /// used for unit testing
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userService"></param>
        /// <param name="contentService"></param>
        /// <param name="nodeId"></param>
        /// <param name="permissionToCheck"></param>
        internal EnsureUserPermissionForContentAttribute(IUser user, IUserService userService, IContentService contentService, int nodeId, char permissionToCheck)
        {
            _user = user;
            _userService = userService;
            _contentService = contentService;
            _nodeId = nodeId;
            _permissionToCheck = permissionToCheck;
        }

        /// <summary>
        /// This constructor will only be able to test the start node access
        /// </summary>
        public EnsureUserPermissionForContentAttribute(int nodeId)
        {
            _nodeId = nodeId;
        }

        public EnsureUserPermissionForContentAttribute(string paramName)
        {
            _paramName = paramName;
            _permissionToCheck = ActionBrowse.Instance.Letter;
        }
        public EnsureUserPermissionForContentAttribute(string paramName, char permissionToCheck)
        {
            _paramName = paramName;
            _permissionToCheck = permissionToCheck;
        }
        
        public override bool AllowMultiple
        {
            get { return true; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (User == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }
            
            if (_nodeId.HasValue == false)
            {
                if (actionContext.ActionArguments[_paramName] == null)
                {
                    throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                }

                _nodeId = (int)actionContext.ActionArguments[_paramName];    
            }
            
            var contentItem = ContentService.GetById(_nodeId.Value);
            if (contentItem == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
            }

            var hasPathAccess = User.HasPathAccess(contentItem);

            if (hasPathAccess == false)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            var permission = UserService.GetPermissions(User, _nodeId.Value).FirstOrDefault();
            if (permission == null || permission.AssignedPermissions.Contains(_permissionToCheck.ToString(CultureInfo.InvariantCulture)))
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }
        }

    }
}