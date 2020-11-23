using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    public class MediaPermissionsQueryStringRequirement : IAuthorizationRequirement
    {
        public MediaPermissionsQueryStringRequirement(string[] paramNames)
        {
            QueryStringNames = paramNames;
        }

        public string[] QueryStringNames { get; }
    }
}
