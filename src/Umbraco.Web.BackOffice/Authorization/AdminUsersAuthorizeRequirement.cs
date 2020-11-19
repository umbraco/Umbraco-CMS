using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirement for the <see cref="AdminUsersAuthorizeHandler"/>
    /// </summary>
    public class AdminUsersAuthorizeRequirement : IAuthorizationRequirement
    {
        public AdminUsersAuthorizeRequirement(string queryStringName = "id")
        {
            QueryStringName = queryStringName;
        }

        public string QueryStringName { get; }
    }
}
