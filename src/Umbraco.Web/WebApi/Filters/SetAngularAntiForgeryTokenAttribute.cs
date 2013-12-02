using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Helpers;
using System.Web.Http.Filters;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// A filter to set the csrf cookie token based on angular conventions
    /// </summary>
    public sealed class SetAngularAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response == null) return;

            string cookieToken, formToken;
            AntiForgery.GetTokens(null, out cookieToken, out formToken);

            //there is a result, set the outgoing cookie
            var cookie = new CookieHeaderValue(AngularAntiForgeryHelper.CookieName, cookieToken)
                {
                    Path = "/",
                    //must be js readable
                    HttpOnly = false,
                    Secure = GlobalSettings.UseSSL
                };
            context.Response.Headers.AddCookies(new[] { cookie });

        }
    }
}