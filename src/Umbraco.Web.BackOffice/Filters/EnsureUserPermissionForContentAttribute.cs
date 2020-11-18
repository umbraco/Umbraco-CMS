using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    ///     Auth filter to check if the current user has access to the content item (by id).
    /// </summary>
    /// <remarks>
    ///     This first checks if the user can access this based on their start node, and then checks node permissions
    ///     By default the permission that is checked is browse but this can be specified in the ctor.
    ///     NOTE: This cannot be an auth filter because that happens too soon and we don't have access to the action params.
    /// </remarks>
    public sealed class EnsureUserPermissionForContentAttribute : TypeFilterAttribute
    {

        /// <summary>
        ///     This constructor will only be able to test the start node access
        /// </summary>
        public EnsureUserPermissionForContentAttribute(int nodeId)
            : base(typeof(EnsureUserPermissionForContentFilter))
        {
            Arguments = new object[]
            {
                nodeId
            };
        }


        public EnsureUserPermissionForContentAttribute(int nodeId, char permissionToCheck)
            : base(typeof(EnsureUserPermissionForContentFilter))
        {
            Arguments = new object[]
            {
                nodeId, permissionToCheck
            };
        }

        public EnsureUserPermissionForContentAttribute(string paramName)
            : base(typeof(EnsureUserPermissionForContentFilter))
        {
            if (paramName == null) throw new ArgumentNullException(nameof(paramName));
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException("Value can't be empty.", nameof(paramName));

            Arguments = new object[]
            {
                paramName, ActionBrowse.ActionLetter
            };
        }


        public EnsureUserPermissionForContentAttribute(string paramName, char permissionToCheck)
            : base(typeof(EnsureUserPermissionForContentFilter))
        {
            if (paramName == null) throw new ArgumentNullException(nameof(paramName));
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException("Value can't be empty.", nameof(paramName));

            Arguments = new object[]
            {
                paramName, permissionToCheck
            };
        }

        private sealed class EnsureUserPermissionForContentFilter : IActionFilter
        {
            private readonly int? _nodeId;
            private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
            private readonly IEntityService _entityService;
            private readonly IUserService _userService;
            private readonly IContentService _contentService;
            private readonly string _paramName;
            private readonly char? _permissionToCheck;

            public EnsureUserPermissionForContentFilter(
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                IEntityService entityService,
                IUserService userService,
                IContentService contentService,
                string paramName)
                :this(backofficeSecurityAccessor, entityService, userService, contentService, null, paramName, ActionBrowse.ActionLetter)
            {

            }

            public EnsureUserPermissionForContentFilter(
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                IEntityService entityService,
                IUserService userService,
                IContentService contentService,
                int nodeId,
                char permissionToCheck)
                :this(backofficeSecurityAccessor, entityService, userService, contentService, nodeId, null, permissionToCheck)
            {

            }

            public EnsureUserPermissionForContentFilter(
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                IEntityService entityService,
                IUserService userService,
                IContentService contentService,
                int nodeId)
                :this(backofficeSecurityAccessor, entityService, userService, contentService, nodeId, null, null)
            {

            }
            public EnsureUserPermissionForContentFilter(
                    IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                    IEntityService entityService,
                    IUserService userService,
                    IContentService contentService,
                    string paramName, char permissionToCheck)
                :this(backofficeSecurityAccessor, entityService, userService, contentService, null, paramName, permissionToCheck)
            {

            }


            private EnsureUserPermissionForContentFilter(
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                IEntityService entityService,
                IUserService userService,
                IContentService contentService,
                int? nodeId, string paramName, char? permissionToCheck)
            {
                _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
                _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
                _userService = userService ?? throw new ArgumentNullException(nameof(userService));
                _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));

                _paramName = paramName;
                if (permissionToCheck.HasValue)
                {
                    _permissionToCheck = permissionToCheck.Value;
                }


                if (nodeId.HasValue)
                {
                    _nodeId = nodeId.Value;
                }
            }



            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser == null)
                {
                    //not logged in
                    throw new HttpResponseException(HttpStatusCode.Unauthorized);
                }

                int nodeId;
                if (_nodeId.HasValue == false)
                {
                    var parts = _paramName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                    if (context.ActionArguments[parts[0]] == null)
                    {
                        throw new InvalidOperationException("No argument found for the current action with the name: " +
                                                            _paramName);
                    }

                    if (parts.Length == 1)
                    {
                        var argument = context.ActionArguments[parts[0]].ToString();
                        // if the argument is an int, it will parse and can be assigned to nodeId
                        // if might be a udi, so check that next
                        // otherwise treat it as a guid - unlikely we ever get here
                        if (int.TryParse(argument, out int parsedId))
                        {
                            nodeId = parsedId;
                        }
                        else if (UdiParser.TryParse(argument, true, out var udi))
                        {
                            nodeId = _entityService.GetId(udi).Result;
                        }
                        else
                        {
                            Guid.TryParse(argument, out Guid key);
                            nodeId = _entityService.GetId(key, UmbracoObjectTypes.Document).Result;
                        }
                    }
                    else
                    {
                        //now we need to see if we can get the property of whatever object it is
                        var pType = context.ActionArguments[parts[0]].GetType();
                        var prop = pType.GetProperty(parts[1]);
                        if (prop == null)
                        {
                            throw new InvalidOperationException(
                                "No argument found for the current action with the name: " + _paramName);
                        }

                        nodeId = (int) prop.GetValue(context.ActionArguments[parts[0]]);
                    }
                }
                else
                {
                    nodeId = _nodeId.Value;
                }

                var permissionResult = ContentPermissionsHelper.CheckPermissions(nodeId,
                    _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                    _userService,
                    _contentService,
                    _entityService,
                    out var contentItem,
                    _permissionToCheck.HasValue ? new[] { _permissionToCheck.Value } : null);

                if (permissionResult == ContentPermissionsHelper.ContentAccess.NotFound)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                if (permissionResult == ContentPermissionsHelper.ContentAccess.Denied)
                {
                    context.Result = new ForbidResult();
                    return;
                }


                if (contentItem != null)
                {
                    //store the content item in request cache so it can be resolved in the controller without re-looking it up
                    context.HttpContext.Items[typeof(IContent).ToString()] = contentItem;
                }

            }

            public void OnActionExecuted(ActionExecutedContext context)
            {

            }


        }
    }
}
