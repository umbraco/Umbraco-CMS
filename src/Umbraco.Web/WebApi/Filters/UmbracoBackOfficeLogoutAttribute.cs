using System;
using System.ComponentModel;
using System.Web.Http.Filters;
using Umbraco.Core.Security;

namespace Umbraco.Web.WebApi.Filters
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future, use OWIN IAuthenticationManager.SignOut instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class UmbracoBackOfficeLogoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {            
            if (context.Response == null) return;
            if (context.Response.IsSuccessStatusCode == false) return;

            //this clears out all of our cookies
            context.Response.UmbracoLogoutWebApi();
            
            //this calls the underlying owin sign out logic - which should call the 
            // auth providers middleware callbacks if using custom auth middleware
            context.Request.TryGetOwinContext().Result.Authentication.SignOut();
        }
    }
}