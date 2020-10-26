using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Editors;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Auth filter to check if the current user has access to the content item
    /// </summary>
    /// <remarks>
    /// Since media doesn't have permissions, this simply checks start node access
    /// </remarks>
    internal sealed class EnsureUserPermissionForMediaAttribute : TypeFilterAttribute
    {
        public EnsureUserPermissionForMediaAttribute(int nodeId)
            : base(typeof(EnsureUserPermissionForMediaFilter))
        {
            Arguments = new object[]
            {
                nodeId
            };
        }

        public EnsureUserPermissionForMediaAttribute(string paramName)
            : base(typeof(EnsureUserPermissionForMediaFilter))
        {
            Arguments = new object[]
            {
                paramName
            };
        }
        private sealed class EnsureUserPermissionForMediaFilter : IActionFilter
        {
            private readonly int? _nodeId;
            private readonly string _paramName;
            private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
            private readonly IEntityService _entityService;
            private readonly IMediaService _mediaService;

            /// <summary>
            /// This constructor will only be able to test the start node access
            /// </summary>
            public EnsureUserPermissionForMediaFilter(
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                IEntityService entityService,
                IMediaService mediaService,
                int nodeId)
                :this(backofficeSecurityAccessor, entityService, mediaService, nodeId, null)
            {
                _nodeId = nodeId;
            }

            public EnsureUserPermissionForMediaFilter(
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                IEntityService entityService,
                IMediaService mediaService,
                string paramName)
                :this(backofficeSecurityAccessor, entityService, mediaService,null, paramName)
            {
                if (paramName == null) throw new ArgumentNullException(nameof(paramName));
                if (string.IsNullOrEmpty(paramName))
                    throw new ArgumentException("Value can't be empty.", nameof(paramName));
            }

            private EnsureUserPermissionForMediaFilter(
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                IEntityService entityService,
                IMediaService mediaService,
                int? nodeId, string paramName)
            {
                _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
                _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
                _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

                _paramName = paramName;

                if (nodeId.HasValue)
                {
                    _nodeId = nodeId.Value;
                }
            }

            private int GetNodeIdFromParameter(object parameterValue)
            {
                if (parameterValue is int)
                {
                    return (int) parameterValue;
                }

                var guidId = Guid.Empty;
                if (parameterValue is Guid)
                {
                    guidId = (Guid) parameterValue;
                }
                else if (parameterValue is GuidUdi)
                {
                    guidId = ((GuidUdi) parameterValue).Guid;
                }

                if (guidId != Guid.Empty)
                {
                    var found = _entityService.GetId(guidId, UmbracoObjectTypes.Media);
                    if (found)
                        return found.Result;
                }

                throw new InvalidOperationException("The id type: " + parameterValue.GetType() +
                                                    " is not a supported id");
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser == null)
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
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
                        nodeId = GetNodeIdFromParameter(context.ActionArguments[parts[0]]);
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

                        nodeId = GetNodeIdFromParameter(prop.GetValue(context.ActionArguments[parts[0]]));
                    }
                }
                else
                {
                    nodeId = _nodeId.Value;
                }

                if (MediaController.CheckPermissions(
                    context.HttpContext.Items,
                    _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                    _mediaService,
                    _entityService,
                    nodeId))
                {

                }
                else
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {

            }

        }
    }
}
