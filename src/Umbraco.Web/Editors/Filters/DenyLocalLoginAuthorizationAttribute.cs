using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Web.WebApi;
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors.Filters
{
    internal class DenyLocalLoginAuthorizationAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var owinContext = actionContext.Request.TryGetOwinContext().Result;

            // no authorization if any external logins deny local login
            return !owinContext.Authentication.HasDenyLocalLogin();
        }
    }
}
