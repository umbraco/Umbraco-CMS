using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirement for the <see cref="UserGroupAuthorizationHandler"/>
    /// </summary>
    public class UserGroupAuthorizeRequirement : IAuthorizationRequirement
    {
        public UserGroupAuthorizeRequirement(string queryStringName = "id")
        {
            QueryStringName = queryStringName;
        }

        public string QueryStringName { get; }
    }
}
