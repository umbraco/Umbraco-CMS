using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.BackOffice.Authorization
{
    public class MediaPermissionsQueryStringHandler : PermissionsQueryStringHandler<MediaPermissionsQueryStringRequirement>
    {
        private readonly MediaPermissions _mediaPermissions;

        public MediaPermissionsQueryStringHandler(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IHttpContextAccessor httpContextAccessor,
            IEntityService entityService,
            MediaPermissions mediaPermissions)
            : base(backofficeSecurityAccessor, httpContextAccessor, entityService)
        {
            _mediaPermissions = mediaPermissions;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, MediaPermissionsQueryStringRequirement requirement)
        {
            if (!HttpContextAccessor.HttpContext.Request.Query.TryGetValue(requirement.QueryStringName, out var routeVal))
            {
                // Must succeed this requirement since we cannot process it.
                return Task.FromResult(true);
            }

            var argument = routeVal.ToString();

            if (!TryParseNodeId(argument, out int nodeId))
            {
                // Must succeed this requirement since we cannot process it.
                return Task.FromResult(true);
            }

            var permissionResult = _mediaPermissions.CheckPermissions(
                BackofficeSecurityAccessor.BackOfficeSecurity.CurrentUser,
                nodeId,
                out var mediaItem);

            if (mediaItem != null)
            {
                // Store the media item in request cache so it can be resolved in the controller without re-looking it up.
                HttpContextAccessor.HttpContext.Items[typeof(IMedia).ToString()] = mediaItem;
            }

            return permissionResult switch
            {
                MediaPermissions.MediaAccess.Denied => Task.FromResult(false),
                _ => Task.FromResult(true),
            };
        }       
    }
}
