using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Clears the angular csrf cookie if the request was successful
    /// </summary>
    public sealed class ClearAngularAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response == null) return;
            if (context.Response.IsSuccessStatusCode == false) return;

            //remove the cookies
            var angularCookie = new CookieHeaderValue(AngularAntiForgeryHelper.AngularCookieName, "null")
                {
                    Expires = DateTime.Now.AddYears(-1),
                    //must be js readable
                    HttpOnly = false,
                    Path = "/"
                };
            var validationCookie = new CookieHeaderValue(AngularAntiForgeryHelper.CsrfValidationCookieName, "null")
            {
                Expires = DateTime.Now.AddYears(-1),
                HttpOnly = true,
                Path = "/"
            };
            context.Response.Headers.AddCookies(new[] { angularCookie, validationCookie });
        }
    }
}