using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Web.Common.ApplicationModels;
using Umbraco.Web.Common.Attributes;

namespace Umbraco.Web.Common.Filters
{

    /// <summary>
    /// Assigned as part of the umbraco back office application model <see cref="UmbracoApiBehaviorApplicationModelProvider"/>
    /// to always ensure that back office authentication occurs for all controller/actions with
    /// <see cref="IsBackOfficeAttribute"/> applied.
    /// </summary>
    public class EnsureUmbracoBackOfficeAuthentication : IAuthorizationFilter, IAuthorizeData
    {
        // Implements IAuthorizeData only to return the back office scheme
        public string AuthenticationSchemes { get; set; } = Umbraco.Core.Constants.Security.BackOfficeAuthenticationType;
        public string Policy { get; set; }
        public string Roles { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
        }
    }
}
