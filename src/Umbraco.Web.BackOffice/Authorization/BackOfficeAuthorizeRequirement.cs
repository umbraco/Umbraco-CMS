using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirement for the <see cref="BackOfficeAuthorizeRequirement"/>
    /// </summary>
    public class BackOfficeAuthorizeRequirement : IAuthorizationRequirement
    {
        public BackOfficeAuthorizeRequirement(bool requireApproval = true)
        {
            RequireApproval = requireApproval;
        }

        public bool RequireApproval { get; }
    }
}
