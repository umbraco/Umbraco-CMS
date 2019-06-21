using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
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
            if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentException("Value cannot be null or whitespace.", "paramName");

            _paramName = paramName;
            _permissionToCheck = ActionBrowse.Instance.Letter;
        }

        public EnsureUserPermissionForContentAttribute(string paramName, char permissionToCheck)
            : this(paramName)
        {
            _permissionToCheck = permissionToCheck;
        }

        public override bool AllowMultiple
        {
            get { return true; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (UmbracoContext.Current.Security.CurrentUser == null)
            {
                //not logged in
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }

            var ignoreUserStartNodes = false;

            if (actionContext.ActionArguments.ContainsKey("dataTypeId"))
            {
                if (actionContext.ActionArguments.TryGetValue("dataTypeId", out var dataTypeIdValue))
                {
                    var dataTypeIdString = dataTypeIdValue?.ToString();
                    if (string.IsNullOrEmpty(dataTypeIdString) == false &&
                        Guid.TryParse(dataTypeIdString, out var dataTypeId))
                    {
                        ignoreUserStartNodes =
                            ApplicationContext.Current.Services.DataTypeService
                                .IsDataTypeIgnoringUserStartNodes(dataTypeId);
                    }
                }

            }

            int nodeId;
            if (_nodeId.HasValue == false)
            {
                var parts = _paramName.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

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
                        nodeId = ApplicationContext.Current.Services.EntityService.GetIdForUdi(udi).Result;
                    }
                    else
                    {
                        Guid.TryParse(argument, out Guid key);
                        nodeId = ApplicationContext.Current.Services.EntityService.GetIdForKey(key, UmbracoObjectTypes.Document).Result;
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

            if (ContentController.CheckPermissions(
                actionContext.Request.Properties,
                UmbracoContext.Current.Security.CurrentUser,
                ApplicationContext.Current.Services.UserService,
                ApplicationContext.Current.Services.ContentService,
                ApplicationContext.Current.Services.EntityService,
                nodeId,
                _permissionToCheck.HasValue ? new[]{_permissionToCheck.Value}: null,
                ignoreUserStartNodes: ignoreUserStartNodes))
            {
                base.OnActionExecuting(actionContext);
            }
            else
            {
                throw new HttpResponseException(actionContext.Request.CreateUserNoAccessResponse());
            }

        }



    }
}
