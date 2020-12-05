using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.BackOffice.Authorization
{
    public abstract class PermissionsQueryStringHandler<T> : MustSatisfyRequirementAuthorizationHandler<T>
        where T : IAuthorizationRequirement
    {
        public PermissionsQueryStringHandler(
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IHttpContextAccessor httpContextAccessor,
            IEntityService entityService)
        {
            BackofficeSecurityAccessor = backofficeSecurityAccessor;
            HttpContextAccessor = httpContextAccessor;
            EntityService = entityService;
        }

        protected IBackOfficeSecurityAccessor BackofficeSecurityAccessor { get; set; }

        protected IHttpContextAccessor HttpContextAccessor { get; set; }

        protected IEntityService EntityService { get; set; }

        protected bool TryParseNodeId(string argument, out int nodeId)
        {
            // If the argument is an int, it will parse and can be assigned to nodeId.
            // It might be a udi, so check that next.
            // Otherwise treat it as a guid - unlikely we ever get here.
            // Failing that, we can't parse it.
            if (int.TryParse(argument, out int parsedId))
            {
                nodeId = parsedId;
                return true;
            }
            else if (UdiParser.TryParse(argument, true, out var udi))
            {
                nodeId = EntityService.GetId(udi).Result;
                return true;
            }
            else if (Guid.TryParse(argument, out var key))
            {
                nodeId = EntityService.GetId(key, UmbracoObjectTypes.Document).Result;
                return true;
            }
            else
            {
                nodeId = 0;
                return false;
            }
        }
    }
}
