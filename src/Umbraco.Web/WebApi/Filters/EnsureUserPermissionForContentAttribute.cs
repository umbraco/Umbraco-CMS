using System;
using System.Collections;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core.Models;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Auth filter to check if the current user has access to the content item (by id). 
    /// </summary>
    /// <remarks>
    /// 
    /// This first checks if the user can access this based on their start node, and then checks node permissions
    /// TODO: Implement start node check!!!
    /// 
    /// By default the permission that is checked is browse but this can be specified in the ctor.
    /// NOTE: This cannot be an auth filter because that happens too soon and we don't have access to the action params.
    /// </remarks>
    internal sealed class EnsureUserPermissionForContentAttribute : ActionFilterAttribute
    {
        private readonly bool _onlyCheckStartNode;
        private readonly string _paramName;
        private readonly char _permissionToCheck;

        public EnsureUserPermissionForContentAttribute(bool onlyCheckStartNode)
        {
            _onlyCheckStartNode = onlyCheckStartNode;            
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
            if (_onlyCheckStartNode)
            {
                //TODO: implement this as well!
            }

            if (UmbracoContext.Current.UmbracoUser == null)
            {
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            if (actionContext.ActionArguments[_paramName] == null)
            {
                throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
            }

            var nodeId = (int)actionContext.ActionArguments[_paramName];
            
            //TODO: Change these calls to a service layer call and make sure we can mock it!
            var permissions = UmbracoContext.Current.UmbracoUser.GetPermissions(nodeId);
            if (permissions.ToCharArray().Contains(_permissionToCheck))
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