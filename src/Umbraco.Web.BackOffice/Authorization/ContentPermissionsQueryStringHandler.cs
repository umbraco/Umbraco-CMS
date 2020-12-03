using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Used to authorize if the user has the correct permission access to the content for the content id specified in a query string
    /// </summary>
    public class ContentPermissionsQueryStringHandler : PermissionsQueryStringHandler<ContentPermissionsQueryStringRequirement>
    {
        private readonly ContentPermissions _contentPermissions;

        public ContentPermissionsQueryStringHandler(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IHttpContextAccessor httpContextAccessor, 
            IEntityService entityService,
            ContentPermissions contentPermissions)
            : base(backofficeSecurityAccessor, httpContextAccessor, entityService)
        {
            _contentPermissions = contentPermissions;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, ContentPermissionsQueryStringRequirement requirement)
        {
            int nodeId;
            if (requirement.NodeId.HasValue == false)
            {
                if (!HttpContextAccessor.HttpContext.Request.Query.TryGetValue(requirement.QueryStringName, out var routeVal))
                {
                    // Must succeed this requirement since we cannot process it
                    return Task.FromResult(true);
                }
                else
                {
                    var argument = routeVal.ToString();

                    if (!TryParseNodeId(argument, out nodeId))
                    {
                        // Must succeed this requirement since we cannot process it.
                        return Task.FromResult(true);
                    }
                }
            }
            else
            {
                nodeId = requirement.NodeId.Value;
            }

            var permissionResult = _contentPermissions.CheckPermissions(nodeId,
                BackofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                out IContent contentItem,
                new[] { requirement.PermissionToCheck });

            if (contentItem != null)
            {
                // Store the content item in request cache so it can be resolved in the controller without re-looking it up.
                HttpContextAccessor.HttpContext.Items[typeof(IContent).ToString()] = contentItem;
            }

            return permissionResult switch
            {
                ContentPermissions.ContentAccess.Denied => Task.FromResult(false),
                _ => Task.FromResult(true),
            };
        }
    }
}
