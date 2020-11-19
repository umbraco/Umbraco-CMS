using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Marker requirement for the <see cref="DenyLocalLoginAuthorizeHandler"/>
    /// </summary>
    public class DenyLocalLoginRequirement : IAuthorizationRequirement
    {
    }
}
