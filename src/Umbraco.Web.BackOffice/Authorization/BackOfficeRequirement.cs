using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirement for the <see cref="BackOfficeRequirement"/>
    /// </summary>
    public class BackOfficeRequirement : IAuthorizationRequirement
    {
        public BackOfficeRequirement(bool requireApproval = true)
        {
            RequireApproval = requireApproval;
        }

        public bool RequireApproval { get; }
    }
}
