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
    public class ContentPermissionsQueryStringHandler : MustSatisfyRequirementAuthorizationHandler<ContentPermissionsQueryStringRequirement>
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEntityService _entityService;
        private readonly ContentPermissions _contentPermissions;

        public ContentPermissionsQueryStringHandler(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IHttpContextAccessor httpContextAccessor, 
            IEntityService entityService,
            ContentPermissions contentPermissions)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _httpContextAccessor = httpContextAccessor;
            _entityService = entityService;
            _contentPermissions = contentPermissions;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, ContentPermissionsQueryStringRequirement requirement)
        {
            int nodeId;
            if (requirement.NodeId.HasValue == false)
            {
                if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue(requirement.QueryStringName, out var routeVal))
                {
                    // Must succeed this requirement since we cannot process it
                    return Task.FromResult(true);
                }
                else
                {
                    var argument = routeVal.ToString();

                    // If the argument is an int, it will parse and can be assigned to nodeId.
                    // It might be a udi, so check that next.
                    // Otherwise treat it as a guid - unlikely we ever get here.
                    // Failing that, we can't parse it so must succeed this requirement since we cannot process it.
                    if (int.TryParse(argument, out int parsedId))
                    {
                        nodeId = parsedId;
                    }
                    else if (UdiParser.TryParse(argument, true, out var udi))
                    {
                        nodeId = _entityService.GetId(udi).Result;
                    }
                    else if (Guid.TryParse(argument, out var key))
                    { 
                        nodeId = _entityService.GetId(key, UmbracoObjectTypes.Document).Result;
                    }
                    else
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            else
            {
                nodeId = requirement.NodeId.Value;
            }

            var permissionResult = _contentPermissions.CheckPermissions(nodeId,
                _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                out IContent contentItem,
                new[] { requirement.PermissionToCheck });

            if (contentItem != null)
            {
                // Store the content item in request cache so it can be resolved in the controller without re-looking it up.
                _httpContextAccessor.HttpContext.Items[typeof(IContent).ToString()] = contentItem;
            }

            return permissionResult switch
            {
                ContentPermissions.ContentAccess.Denied => Task.FromResult(false),
                _ => Task.FromResult(true),
            };
        }
    }
}
