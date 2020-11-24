using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    public class MediaPermissionsQueryStringRequirement : IAuthorizationRequirement
    {
        public MediaPermissionsQueryStringRequirement(string paramName)
        {
            QueryStringName = paramName;
        }

        public string QueryStringName { get; }
    }
}
