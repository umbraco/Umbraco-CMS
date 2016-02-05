using System;
using System.ComponentModel;
using System.Web.Http.Filters;
using Umbraco.Core.Security;

namespace Umbraco.Web.WebApi.Filters
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future, use OWIN IAuthenticationManager.SignOut instead", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class UmbracoBackOfficeLogoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            throw new NotSupportedException("This method is not supported and should not be used, it has been removed in Umbraco 7.4");
        }
    }
}