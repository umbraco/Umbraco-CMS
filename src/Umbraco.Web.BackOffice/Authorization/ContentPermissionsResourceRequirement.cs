using Microsoft.AspNetCore.Authorization;
using Umbraco.Web.Actions;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// An authorization requirement for <see cref="ContentPermissionsResourceHandler"/>
    /// </summary>
    public class ContentPermissionsResourceRequirement : IAuthorizationRequirement
    {
    }
}
