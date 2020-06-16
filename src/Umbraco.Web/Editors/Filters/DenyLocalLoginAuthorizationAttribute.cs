using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors.Filters
{
    internal class DenyLocalLoginAuthorizationAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var owinContext = actionContext.Request.TryGetOwinContext().Result;

            // no authorization if any external logins deny local login
            if (owinContext.Authentication.GetExternalAuthenticationTypes().Any(p => p.Properties.ContainsKey("UmbracoBackOffice_DenyLocalLogin")))
                return false;

            return true;
        }
    }
}
