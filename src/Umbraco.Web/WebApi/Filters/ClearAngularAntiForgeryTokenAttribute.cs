using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi.Filters
{
    public sealed class ClearAngularAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response == null) return;

            //remove the cookie
            var cookie = new CookieHeaderValue(AngularAntiForgeryHelper.CookieName, "null")
                {
                    Expires = DateTime.Now.AddYears(-1),
                    //must be js readable
                    HttpOnly = false,
                    Path = "/"
                };
            context.Response.Headers.AddCookies(new[] { cookie });
        }
    }
}