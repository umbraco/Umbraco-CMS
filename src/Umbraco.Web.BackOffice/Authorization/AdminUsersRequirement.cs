using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirement for the <see cref="AdminUsersHandler"/>
    /// </summary>
    public class AdminUsersRequirement : IAuthorizationRequirement
    {
        public AdminUsersRequirement(string queryStringName = "id")
        {
            QueryStringName = queryStringName;
        }

        public string QueryStringName { get; }
    }
}
