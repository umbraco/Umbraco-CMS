using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirement for the <see cref="UserGroupHandler"/>
    /// </summary>
    public class UserGroupRequirement : IAuthorizationRequirement
    {
        public UserGroupRequirement(string queryStringName = "id")
        {
            QueryStringName = queryStringName;
        }

        public string QueryStringName { get; }
    }
}
