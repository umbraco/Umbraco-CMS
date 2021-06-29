using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Web.Actions;
using Umbraco.Web.Composing;

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
    public sealed class EnsureUserPermissionForContentAttribute : ActionFilterAttribute
    {
        private readonly int? _nodeId;
        private readonly string _paramName;
        private readonly char? _permissionToCheck;

        /// <summary>
        /// This constructor will only be able to test the start node access
        /// </summary>
        public EnsureUserPermissionForContentAttribute(int nodeId)
        {
            _nodeId = nodeId;
        }

        public EnsureUserPermissionForContentAttribute(int nodeId, char permissionToCheck)
            : this(nodeId)
        {
            _permissionToCheck = permissionToCheck;
        }

        public EnsureUserPermissionForContentAttribute(string paramName)
        {
            if (paramName == null) throw new ArgumentNullException(nameof(paramName));
            if (string.IsNullOrEmpty(paramName)) throw new ArgumentException("Value can't be empty.", nameof(paramName));

            _paramName = paramName;
            _permissionToCheck = ActionBrowse.ActionLetter;
        }

        public EnsureUserPermissionForContentAttribute(string paramName, char permissionToCheck)
            : this(paramName)
        {
            _permissionToCheck = permissionToCheck;
        }

        public override bool AllowMultiple => true;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (Current.UmbracoContext.Security.CurrentUser == null)
            {
                //not logged in
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            int nodeId;
            if (_nodeId.HasValue == false)
            {
                var parts = _paramName.Split(Constants.CharArrays.Period, StringSplitOptions.RemoveEmptyEntries);

                if (actionContext.ActionArguments[parts[0]] == null)
                {
                    throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                }

                if (parts.Length == 1)
                {
                    var argument = actionContext.ActionArguments[parts[0]].ToString();
                    // if the argument is an int, it will parse and can be assigned to nodeId
                    // if might be a udi, so check that next
                    // otherwise treat it as a guid - unlikely we ever get here
                    if (int.TryParse(argument, out int parsedId))
                    {
                        nodeId = parsedId;
                    }
                    else if (Udi.TryParse(argument, true, out Udi udi))
                    {
                        // TODO: inject? we can't because this is an attribute but we could provide ctors and empty ctors that pass in the required services
                        nodeId = Current.Services.EntityService.GetId(udi).Result;
                    }
                    else
                    {
                        Guid.TryParse(argument, out Guid key);
                        // TODO: inject? we can't because this is an attribute but we could provide ctors and empty ctors that pass in the required services
                        nodeId = Current.Services.EntityService.GetId(key, UmbracoObjectTypes.Document).Result;
                    }
                }
                else
                {
                    //now we need to see if we can get the property of whatever object it is
                    var pType = actionContext.ActionArguments[parts[0]].GetType();
                    var prop = pType.GetProperty(parts[1]);
                    if (prop == null)
                    {
                        throw new InvalidOperationException("No argument found for the current action with the name: " + _paramName);
                    }
                    nodeId = (int)prop.GetValue(actionContext.ActionArguments[parts[0]]);
                }
            }
            else
            {
                nodeId = _nodeId.Value;
            }

            var permissionResult = ContentPermissionsHelper.CheckPermissions(
                nodeId,
                Current.UmbracoContext.Security.CurrentUser,
                Current.Services.UserService,
                Current.Services.ContentService,
                Current.Services.EntityService,
                Current.AppCaches,
                out var contentItem,
                _permissionToCheck.HasValue ? new[] { _permissionToCheck.Value } : null);

            if (permissionResult == ContentPermissionsHelper.ContentAccess.NotFound)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (permissionResult == ContentPermissionsHelper.ContentAccess.Denied)
                throw new HttpResponseException(actionContext.Request.CreateUserNoAccessResponse());

            if (contentItem != null)
            {
                //store the content item in request cache so it can be resolved in the controller without re-looking it up
                actionContext.Request.Properties[typeof(IContent).ToString()] = contentItem;
            }

            base.OnActionExecuting(actionContext);
        }
    }
}
