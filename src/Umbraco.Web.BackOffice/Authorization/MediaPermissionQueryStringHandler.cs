using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.BackOffice.Authorization
{
    public class MediaPermissionQueryStringHandler : AuthorizationHandler<MediaPermissionsQueryStringRequirement>
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MediaPermissions _mediaPermissions;
        private readonly IEntityService _entityService;

        public MediaPermissionQueryStringHandler(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IHttpContextAccessor httpContextAccessor,
            MediaPermissions mediaPermissions)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _httpContextAccessor = httpContextAccessor;
            _mediaPermissions = mediaPermissions;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MediaPermissionsQueryStringRequirement requirement)
        {
            StringValues routeVal;
            foreach (var qs in requirement.QueryStringNames)
            {
                if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue(qs, out routeVal))
                {
                    break;
                }
            }

            if (routeVal.Count == 0)
            {
                throw new InvalidOperationException("No argument found for the current action with by names " + string.Join(", ", requirement.QueryStringNames));
            }

            int nodeId;

            var argument = routeVal.ToString();
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

            var permissionResult = _mediaPermissions.CheckPermissions(
                _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                nodeId,
                out var mediaItem);

            if (permissionResult == MediaPermissions.MediaAccess.NotFound)
            {
                return null;
            }

            if (permissionResult == MediaPermissions.MediaAccess.Denied)
            {
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }

            if (mediaItem != null)
            {
                //store the content item in request cache so it can be resolved in the controller without re-looking it up
                _httpContextAccessor.HttpContext.Items[typeof(IMedia).ToString()] = mediaItem;
            }

            return Task.CompletedTask;
        }
    }
}
